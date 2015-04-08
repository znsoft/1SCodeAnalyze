using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    class SerializedList : SerializedItem
    {
        public SerializedList(String Content) : base(Content)
        {
            Parsed = false;
            Children = new List<SerializedItem>();
        }

        public override List<SerializedItem> Items
        {
            get
            {
                if (!Parsed)
                    Parse();

                return Children;
            }
        }

        private void Parse()
        {
            Read(RawContent, ref Children);
            Parsed = true;
        }

        public SerializedList DrillDown(int BraceCount)
        {
            bool Success = false;
            int StartPos = 0;

            for (int i = 1; i < RawContent.Length && BraceCount > 0; ++i)
            {
                if(RawContent[i] == '{')
                {
                    --BraceCount;
                }

                if(BraceCount == 0)
                {
                    Success = true;
                    StartPos = i;
                    break;
                }

            }

            if (Success)
            {
                int EndPos = RewindList(RawContent, StartPos);
                return new SerializedList(RawContent.Substring(StartPos, EndPos - StartPos));
            }
            else
                throw new ArgumentException();

        }

        private List<SerializedItem> Children;
        private bool Parsed;

        public static String StripQuotes(String InitialString)
        {
            if (InitialString != String.Empty && InitialString[0] == '\"')
                return InitialString.Substring(1, InitialString.Length - 2);
            else
                return InitialString;
        }

        private static int RewindList(String Content, int Start)
        {
            int BraceCount = 0;
            int i = Start;

            for (; i < Content.Length; ++i)
            {
                Char CurrentChar = Content[i];

                if (CurrentChar == '{')
                {
                    BraceCount++;
                }
                else if (CurrentChar == '}')
                {
                    BraceCount--;
                }

                if (BraceCount == 0)
                    break;

            }

            return i+1;

        }

        private static String ConvertQuotedString(String QuotedString, ref int Position)
        {

            if (QuotedString.Length == 0)
            {
                return String.Empty;
            }

            if (QuotedString[0] != '"')
                throw new ArgumentException();

            int startPos = Position;
            int endPos = -1;
            Char Quot = '"';

            for (int i = 1; i < QuotedString.Length; ++i)
            {
                Char CurrentChar = QuotedString[i];
                if (CurrentChar == Quot)
                {

                    if (i + 1 < QuotedString.Length)
                    {
                        Char NextChar = QuotedString[i + 1];
                        if (NextChar == Quot)
                            i++;
                        else
                        {
                            endPos = i;
                            break;
                        }
                    }
                    else
                    {
                        endPos = i;
                        break;
                    }

                }
            }

            if (endPos == -1)
            {
                throw new ArgumentException();
            }

            Position = startPos + endPos + 1;

            if (QuotedString != String.Empty)
            {
                StringBuilder bldr = new StringBuilder(QuotedString, 1, endPos-1, 500);

                bldr.Replace("\"\"", "\"");

                return bldr.ToString();
            }
            else
                return String.Empty;

        }

        private static void Read(String Content, ref List<SerializedItem> Destination)
        {
            
            if(Content[0] != '{')
                throw new ArgumentException("Wrong content. List must start with a brace '{'");

            Char[] delimiters = {
                                    ',',
                                    '}'
                                };

            for (int i = 0; i < Content.Length; ++i)
            {
                Char CurrentChar = Content[i];

                if (CurrentChar == '{')
                {
                    if (i > 0)
                    {
                        int ListEnd = RewindList(Content, i);

                        SerializedList lst = new SerializedList(Content.Substring(i, ListEnd-i));
                        Destination.Add(lst);
                        i = ListEnd;

                    }
                }
                else if(CurrentChar == '}')
                {
                }
                else if(CurrentChar !='\n' && CurrentChar !='\r')
                {
                    int delimPos = Content.IndexOfAny(delimiters, i);
                    if (delimPos < 0)
                    {
                        // элемент ничем не заканчивается.
                        throw new ArgumentException("Stream format error");
                    }
                    else if (delimPos != i)
                    {

                        if (CurrentChar == '"')
                        {
                            String initial = Content.Substring(i);
                            String Value = ConvertQuotedString(initial, ref i);
                            Destination.Add(new SerializedItem(Value));
                        }
                        else
                        {
                            String ElementValue = Content.Substring(i, delimPos - i);
                            Destination.Add(new SerializedItem(ElementValue));
                            i = delimPos;
                        }
                    }


                }

            }
        }
    
    }

    static class SLXMLConvertor
    {

        static public String Convert(String Content)
        {
            StringBuilder Result = new StringBuilder();

            Char[] delimiters = { ',', '}' };

            for (int i = 0; i < Content.Length; ++i)
            {
                Char CurrentChar = Content[i];

                if (CurrentChar == '{')
                {
                    Result.Append("<element>");
                }
                else if (CurrentChar == '}')
                {
                    Result.Append("</element>");
                }
                else
                {
                    if (!Char.IsWhiteSpace(CurrentChar))
                    {

                        int delimPos = Content.IndexOfAny(delimiters, i);
                        if (delimPos < 0)
                        {
                            // элемент ничем не заканчивается.
                            throw new ArgumentException("Stream format error");
                        }
                        else if (delimPos != i)
                        {
                            String Value;
                            if (CurrentChar == '"')
                                Value = ConvertQuotedString(Content.Substring(i), ref i);
                            else
                            {
                                Value = ConvertString(Content.Substring(i, delimPos - i));
                                i = delimPos;
                            }

                            Result.AppendFormat("<data>{0}</data>", Value);

                        }

                        if (Content[delimPos] == '}')
                        {
                            Result.Append("</element>");
                        }

                    }
                }

            }

            return Result.ToString();
        }
        
        static public String Convert(SerializedList List)
        {
            
            String Content = List.ToString();
            return Convert(Content);
            
        }

        static private String ConvertQuotedString(String QuotedString, ref int Position)
        {

            if (QuotedString.Length == 0)
            {
                return String.Empty;
            }
            
            if (QuotedString[0] != '"')
                throw new ArgumentException();

            bool Opened = true;
            int level = 1;
            int startPos = Position;
            int endPos = -1;
            Char Quot = '"';

            for (int i = 1; i < QuotedString.Length; ++i)
            {
                Char CurrentChar = QuotedString[i];
                if (CurrentChar == Quot)
                {
                    Opened = !Opened;

                    if (Opened)
                    {
                        level++;
                    }
                    else
                    {
                        level--;
                    }

                    if (level == 0)
                    {
                        endPos = i;
                        break;
                    }

                }
            }

            if (endPos == -1)
            {
                throw new ArgumentException();
            }

            Position = startPos + endPos + 1;

            StringBuilder bld = new StringBuilder(QuotedString, 1, endPos-1, 100);
            bld.Replace("\"\"","\"");
            ReplaceSpecialChars(bld);

            return bld.ToString();

        }
        
        static private String ConvertString(String InValue)
        {
            StringBuilder bld = new StringBuilder(InValue);
            ReplaceSpecialChars(bld);
            return bld.ToString();
        }

        static private void ReplaceSpecialChars(StringBuilder Builder)
        {
            Builder.Replace("&", "&amp;");
            Builder.Replace("<", "&lt;");
            Builder.Replace(">", "&gt;");
        }

    }

}
