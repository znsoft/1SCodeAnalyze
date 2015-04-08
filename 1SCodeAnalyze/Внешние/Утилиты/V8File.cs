using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Runtime.InteropServices;

namespace CFReader
{

    public class V8File : IDisposable
    {
        public V8File(string FileName)
        {
            var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            mmFile = MemoryMappedFile.CreateFromFile(fs, null, 0, MemoryMappedFileAccess.Read, null, HandleInheritability.None, false);
            FileImage = new V8CompressedImage(mmFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read));
        }

        public IImageLister GetLister()
        {
            return FileImage;
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        ~V8File()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (mmFile != null)
            {
                lock (mmFile)
                {
                    mmFile.Dispose();
                    FileImage = null;
                    mmFile = null;
                }
            }
        }

        private V8Image FileImage;
        private MemoryMappedFile mmFile;
    }

    internal class V8Image : IImageLister
    {
        public V8Image(Stream MemStream)
        {
            m_Mem = MemStream;
        }

        protected Stream m_Mem;

        #region IImageLister Members

        private Dictionary<string, V8ItemHandle> m_ItemsMap;

        private void MemReadFrom(int position, int len, ref byte[] dest)
        {
            m_Mem.Seek(position, SeekOrigin.Begin);
            m_Mem.Read(dest, 0, len);
        }

        private TStruct MemReadStruct<TStruct>(int position)
        {
            m_Mem.Seek(position, SeekOrigin.Begin);
            return Helpers.ReadStruct<TStruct>(m_Mem);
        }

        private void FillDataItems()
        {
            m_ItemsMap = new Dictionary<string, V8ItemHandle>();

            int startAddr = stFileHeader.Size;

            stElemAddr[] toc = ReadTableOfContents(startAddr);

            unsafe
            {
                for (int i = 0; i < toc.Length; i++)
                {
                    var tocItem = toc[i];

                    stBlockHeader itemHdr = MemReadStruct<stBlockHeader>((int)tocItem.elem_header_addr);
                    if (!itemHdr.Check())
                    {
                        throw new V8WrongFileException();
                    }

                    int titleSize = (int)Helpers.FromHexStr((sbyte*)itemHdr.data_size_hex);
                    int titleDelta = (int)stBlockHeader.Size + stElemHeaderPrefix.Size;
                    int nameSize = titleSize - stElemHeaderPrefix.Size - 4;

                    byte[] arr = new byte[nameSize];
                    MemReadFrom((int)tocItem.elem_header_addr + titleDelta, nameSize, ref arr);

                    string itemName;
                    fixed (byte* ptr = arr)
                    {
                        itemName = new string((sbyte*)ptr, 0, (int)nameSize, Encoding.Unicode);
                    }

                    V8ItemHandle itemHandle = new V8ItemHandle();
                    itemHandle.Container = this;
                    itemHandle.Name = itemName;

                    if (tocItem.elem_data_addr != 0x7fffffff)
                    {
                        // длина тела данных
                        itemHdr = MemReadStruct<stBlockHeader>((int)tocItem.elem_data_addr);
                        if (!itemHdr.Check())
                        {
                            throw new V8WrongFileException();
                        }

                        uint BodySize = Helpers.FromHexStr((sbyte*)itemHdr.data_size_hex);

                        itemHandle.Offset = tocItem.elem_data_addr;
                        itemHandle.Length = BodySize;

                    }
                    else
                    {
                        itemHandle.Offset = 0;
                        itemHandle.Length = 0;
                    }

                    m_ItemsMap.Add(itemName, itemHandle);

                }
            }

        }

        private stElemAddr[] ReadTableOfContents(int startAddr)
        {

            byte[] buffer = ReadChunkedBlock(startAddr);
            stElemAddr[] TOC = new stElemAddr[buffer.Length / stElemAddr.Size];

            var BufferStream = new MemoryStream(buffer);
            for (int i = 0; i < TOC.Length; i++)
            {
                TOC[i] = Helpers.ReadStruct<stElemAddr>(BufferStream);
            }

            return TOC;

        }

        private byte[] ReadChunkedBlock(int startAddr)
        {
            var pageAddr = startAddr;

            var blockHdr = MemReadStruct<stBlockHeader>(pageAddr);
            if (!blockHdr.Check())
            {
                throw new V8WrongFileException();
            }

            int dataSize = 0;
            int pageSize = 0;
            int NextPage = 0;

            unsafe
            {
                dataSize = (int)Helpers.FromHexStr((sbyte*)blockHdr.data_size_hex);
                pageSize = (int)Helpers.FromHexStr((sbyte*)blockHdr.page_size_hex);
                NextPage = (int)Helpers.FromHexStr((sbyte*)blockHdr.next_page_addr_hex);

            }
            
            if (dataSize == 0)
                return null;

            int bytesRead = 0;
            int readPtr = pageAddr + stBlockHeader.Size;

            byte[] readBuffer = new byte[dataSize];
            int bufferOffset = 0;

            unsafe
            {
                while (bytesRead < dataSize)
                {
                    int tail = dataSize - bytesRead;
                    int readSize = (tail < pageSize) ? tail : pageSize;

                    m_Mem.Seek(readPtr, SeekOrigin.Begin);
                    m_Mem.Read(readBuffer, bufferOffset, readSize);

                    bytesRead += readSize;
                    bufferOffset += readSize;

                    if (NextPage != 0x7fffffff)
                    {
                        readPtr = NextPage + stBlockHeader.Size;

                        blockHdr = MemReadStruct<stBlockHeader>(NextPage);
                        pageSize = (int)Helpers.FromHexStr((sbyte*)blockHdr.page_size_hex);
                        NextPage = (int)Helpers.FromHexStr((sbyte*)blockHdr.next_page_addr_hex);

                    }

                }
            }

            return readBuffer;

        }

        public IEnumerable<V8ItemHandle> Items
        {
            get 
            {
                if (m_ItemsMap == null)
                {
                    FillDataItems();
                }

                return m_ItemsMap.Values;
            }

        }

        public V8ItemHandle GetItem(string ItemName)
        {
            if (m_ItemsMap == null)
            {
                FillDataItems();
            }

            V8ItemHandle handle;
            if (!m_ItemsMap.TryGetValue(ItemName, out handle))
            {
                throw new V8ItemNotFoundException(ItemName);
            }

            return handle;

        }

        #endregion

        internal virtual Stream GetDataStream(V8ItemHandle Handle)
        {
            if (Handle.Container != this)
            {
                throw new ArgumentException("Handle does not belong to an image");
            }

            if (Handle.Length == 0)
                return new MemoryStream();

            byte[] buffer = ReadChunkedBlock((int)Handle.Offset);

            var ItemStream = new MemoryStream(buffer);

            return ItemStream;

        }
    }

    internal class V8CompressedImage : V8Image
    {
        internal V8CompressedImage(Stream srcStream)
            : base(srcStream)
        {
        }

        internal override Stream GetDataStream(V8ItemHandle Handle)
        {
            
            MemoryStream resultStream = new MemoryStream();

            using (var ReadStream = base.GetDataStream(Handle))
            {
                using (var DeflateStream = new System.IO.Compression.DeflateStream(ReadStream, System.IO.Compression.CompressionMode.Decompress))
                {
                    DeflateStream.CopyTo(resultStream);
                }
            }

            resultStream.Position = 0;
            return resultStream;

        }
    }

    public class V8DataElement
    {
        internal V8DataElement(V8ItemHandle handle) : this(handle, null)
        {            
        }

        internal V8DataElement(V8ItemHandle handle, byte[] data)
        {
            m_handle = handle;
            m_data = data;
        }

        public string Name
        {
            get
            {
                return m_handle.Name;
            }
        }

        public Stream GetDataStream()
        {
            if (m_data == null)
            {
                return m_handle.Container.GetDataStream(m_handle);
            }
            else
            {
                return new MemoryStream(m_data);
            }
            
        }

        public override string ToString()
        {
            return Name;
        }

        protected V8ItemHandle m_handle;
        protected byte[] m_data;

        static internal V8DataElement Create(V8ItemHandle handle)
        {
            using (var DataStream = handle.Container.GetDataStream(handle))
            {
                byte[] data = new byte[DataStream.Length];
                DataStream.Read(data, 0, data.Length);
                DataStream.Position = 0;

                if (DataStream.Length >= sizeof(UInt32))
                {
                    byte[] signature = new byte[] { 0xff, 0xff, 0xff, 0x7f };
                    
                    //var strReader = new BinaryReader(DataStream);
                    //UInt32 signature = strReader.ReadUInt32();
                    
                    if (ArrayStartsWith(data, signature))
                    {
                        // Это правильный заголовок блока, значит, данные - несжатый cf-файл.
                        return new V8ContainerElement(handle, data);
                    }
                    else
                    {
                        // Это сырые данные
                        return new V8DataElement(handle, data);
                    }
                }
                else
                {
                    // Это сырые данные
                    return new V8DataElement(handle);
                }
            }
        }

        static private bool ArrayStartsWith(byte[] arr, byte[] signature)
        {
            for (int i = 0; i < signature.Length; i++)
            {
                if (arr[i] != signature[i])
                    return false;
            }

            return true;
        }

    }

    public class V8ContainerElement : V8DataElement, IImageLister
    {
        internal V8ContainerElement(V8ItemHandle handle) : this(handle, null)
        {            
        }

        internal V8ContainerElement(V8ItemHandle handle, byte[] data) : base(handle, data)
        {
            m_ParentContainer = handle.Container;
            m_FoldedImage = new V8Image(m_ParentContainer.GetDataStream(handle));
        }

        private V8Image m_FoldedImage;
        private V8Image m_ParentContainer;

        #region IImageLister Members

        public IEnumerable<V8ItemHandle> Items
        {
            get 
            {
                return m_FoldedImage.Items;
            }
        }

        public V8ItemHandle GetItem(string ItemName)
        {
            return m_FoldedImage.GetItem(ItemName);
        }

        #endregion
    }

    public interface IImageLister
    {
        IEnumerable<V8ItemHandle> Items { get; }
        V8ItemHandle GetItem(string ItemName);
    }

    public struct V8ItemHandle
    {
        public string Name;

        public V8DataElement GetElement()
        {
            return V8DataElement.Create(this);
        }

        public override string ToString()
        {
            return Name;
        }

        internal V8Image Container;
        internal UInt32 Offset;
        internal UInt32 Length;

    }

    public class V8WrongFileException : Exception
    {
        public V8WrongFileException()
            : base("Wrong file format")
        {

        }
    }

    public class V8ItemNotFoundException : Exception
    {
        public V8ItemNotFoundException(string ItemName) : base (String.Format("Item not found {0}", ItemName))
        {

        }
    }

    internal static class Helpers
    {
        public static uint FromHexStr(string hexStr)
        {
            UInt32 value = UInt32.Parse(hexStr, System.Globalization.NumberStyles.AllowHexSpecifier);
            return value;
        }

        public static unsafe uint FromHexStr(sbyte* bArr)
        {
            return FromHexStr(new String(bArr, 0, 8));
        }

        public static T ReadStruct<T>(Stream s)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            s.Read(buffer, 0, Marshal.SizeOf(typeof(T)));

            GCHandle handle;
            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T temp = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return temp;
            
        }

    }

    ////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////
    // Unsafe section
    //
    #region unsafe structures

    unsafe struct stFileHeader
    {
        public UInt32 next_page_addr;
        public UInt32 page_size;
        public UInt32 storage_ver;
        public UInt32 reserved; // всегда 0x00000000 ?
        public const int Size = 16;
    };

    unsafe struct stElemAddr
    {
        public UInt32 elem_header_addr;
        public UInt32 elem_data_addr;
        public UInt32 fffffff; //всегда 0x7fffffff ?
        public const int Size = 12;
    };

    unsafe struct stBlockHeader
    {
        public byte EOL_0D;
        public byte EOL_0A;
        public fixed byte data_size_hex[8];
        public byte space1;
        public fixed byte page_size_hex[8];
        public byte space2;
        public fixed byte next_page_addr_hex[8];
        public byte space3;
        public byte EOL2_0D;
        public byte EOL2_0A;
        public const int Size = 31;

        public bool Check()
        {
            if (EOL_0D != 0x0d ||
                EOL_0A != 0x0a ||
                space1 != 0x20 ||
                space2 != 0x20 ||
                space3 != 0x20 ||
                EOL2_0D != 0x0d ||
                EOL2_0A != 0x0a)
            {
                return false;
            }

            return true;
        }

    };

    unsafe struct stElemHeaderPrefix
    {
        public UInt64 date_creation;
        public UInt64 date_modification;
        public UInt32 res; // всегда 0x000000?
        //изменяемая длина имени блока
        //после имени DWORD res; // всегда 0x000000?
        public const int Size = 20;
    };

    #endregion

}
