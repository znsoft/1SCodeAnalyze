using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace _1SCodeAnalyze
{
    class Program
    {
        static void Main(string[] args)
        {
			Console.Write("Программа для синтаксического анализа кода конфигурации 1С\nдля работы необходимо указать путь к выгруженным файлам конфигурации\nИначе будут проверены все локальные файлы\n");
			String ИмяПапки = AppDomain.CurrentDomain.BaseDirectory;
			if(args.Count()>0)ИмяПапки = args[1];

			var dir=new DirectoryInfo(ИмяПапки);// папка с файлами 
			var files = new List<FileInfo>(); // список для имен файлов 
            ПолучитьФайлыРекурсивно(dir, files);
			АнализаторКода1С Анализ = new АнализаторКода1С(files);
			
        }

		private static void ПолучитьФайлыРекурсивно(DirectoryInfo dir, List<FileInfo> files)
        {
            foreach (FileInfo file in dir.GetFiles()) // извлекаем все файлы и кидаем их в список 
				if(file.Extension.ToUpper().Contains("TXT")||file.Extension == "")files.Add(file); // получаем полный путь к файлу и кидаем его в список 
            foreach (DirectoryInfo directory in dir.GetDirectories())  
                ПолучитьФайлыРекурсивно(directory,files);
            
        }



    }
}
