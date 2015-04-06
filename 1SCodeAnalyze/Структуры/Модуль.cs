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
            ТаблицаАнализа.Add(new ИнформацияАнализа(Index, Проблема, Проблема));
            ЕстьОшибки = true;
            return this;
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
			return this;
		}

        private String СчитатьСодержимоеФайла(FileInfo file)
        {
            var Str = file.OpenText();
            return Str.ReadToEnd();
        }


    }
}
