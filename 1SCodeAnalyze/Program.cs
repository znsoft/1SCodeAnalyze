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
			new КоманднаяСтрока (args);
			var dir=new DirectoryInfo(КоманднаяСтрока.ИмяПапки);// папка с файлами 
			var files = new List<FileInfo>(); // список для имен файлов 
			ПолучитьФайлыРекурсивно(dir, files, КоманднаяСтрока.ext1);
			System.Console.WriteLine ("Будет обработано " + files.Count.ToString () + " файлов. это может занять несколько минут.\n Анализ ...");
			АнализаторКода1С Анализ = new АнализаторКода1С(files);
			
        }

		private static void ПолучитьФайлыРекурсивно(DirectoryInfo dir, List<FileInfo> files, string arg1)
        {
            foreach (FileInfo file in dir.GetFiles()) // извлекаем все файлы и кидаем их в список 
			if (file.Extension.ToUpper ().Contains ("TXT") || file.Extension == "" || file.Extension == arg1||file.Extension == ("."+arg1)) {
					files.Add (file); // получаем полный путь к файлу и кидаем его в список 
				//System.Console.WriteLine (file.Extension);
			}
            foreach (DirectoryInfo directory in dir.GetDirectories())  
				ПолучитьФайлыРекурсивно(directory,files, arg1);
            
        }



    }
}
