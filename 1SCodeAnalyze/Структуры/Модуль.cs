using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace _1SCodeAnalyze.Структуры
{



	/// <summary>
	/// Модуль. это основной класс, хранит и анализирует весь текст модуля объекта (содержимое файла), 
	/// также содержит таблицу анализа этого модуля с проблеммными местами, содержит методы анализа кода
	/// </summary>
    class Модуль
    {
        public FileInfo file;
        public List<ИнформацияАнализа> ТаблицаАнализа;
        public String Текст;
        public Boolean ЕстьОшибки;
		public static Dictionary<String, СвойстваМетодов> МетодыМодуля;

        public Модуль(FileInfo file)
        {
            this.file = file;
            Текст = СчитатьСодержимоеФайла(file);
            ЕстьОшибки = false;
            ТаблицаАнализа = new List<ИнформацияАнализа>();
			МетодыМодуля = new Dictionary<String, СвойстваМетодов>();
        }

        public Модуль ДобавитьПроблему(String Проблема, int Index)
        {
            return ДобавитьПроблему(new ИнформацияАнализа(Index, Проблема, Проблема));
        }

		public Модуль ДобавитьМетод(String ИмяМетода, СвойстваМетодов Свойства)
        {
			if (!МетодыМодуля.ContainsKey(ИмяМетода))
			{
				МетодыМодуля.Add(ИмяМетода, Свойства);
			}
			return this;
        }



		public Модуль ДобавитьПроблему(ИнформацияАнализа Проблема)
		{
			ТаблицаАнализа.Add(Проблема);
			ЕстьОшибки = true;
            //Console.WriteLine(Проблема.ОписаниеПроблемы);
			return this;
		}

        private String СчитатьСодержимоеФайла(FileInfo file)
        {
            var Str = file.OpenText();
            return Str.ReadToEnd();
        }



        public int ПолучитьНомерСтрокиПоИндексу(int Index) {
            return new Regex(@"\n", RegexOptions.Multiline).Matches(Текст.Substring(0, Index)).Count;

            //return Текст.Substring(0, Index).   Split(new char[] { '\n' }, StringSplitOptions.None).                Count();
        }


    }
}
