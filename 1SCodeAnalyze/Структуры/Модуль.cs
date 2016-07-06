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
        public Dictionary<String, СвойстваМетодов> СловарьМетодов;
        int всегоСтрок;
		int строкКомментарии;

        public Модуль(FileInfo file)
        {
            this.file = file;
            Текст = СчитатьСодержимоеФайла(file);
            ЕстьОшибки = false;
            ТаблицаАнализа = new List<ИнформацияАнализа>();
            СловарьМетодов = new Dictionary<String, СвойстваМетодов>();
			if (!КоманднаяСтрока.isAnalyzeCode)
				return;
			НайтиВсеФункцииИПроцедуры();

            foreach (var Метод in СловарьМетодов) {
                if (Метод.Key.Substring(0, 1) == Метод.Key.Substring(0, 1).ToLower()) ДобавитьПроблему("Имя процедуры или функции начинается с маленькой буквы "+Метод.Key, Метод.Value.Index);
            }

        }


        public Модуль ДобавитьПроблему(String Проблема, int Index)
        {


            return ДобавитьПроблему(new ИнформацияАнализа(Index, Проблема, Проблема));
        }

		public Модуль ДобавитьПроблему(ИнформацияАнализа Проблема)
		{

			ТаблицаАнализа.Add(Проблема);
			ЕстьОшибки = true;
            //Console.WriteLine("строка " + ПолучитьНомерСтрокиПоИндексу(Проблема.Смещение) + ": \n" + Проблема.ОписаниеПроблемы + "\n");
			return this;
		}

        private String СчитатьСодержимоеФайла(FileInfo file)
        {
            var Str = file.OpenText();
            return Str.ReadToEnd();
        }

        public int ПолучитьНомерСтрокиПоИндексу(int Index) {
			if(Index == -1) return new Regex(@"\n", RegexOptions.Multiline).Matches(Текст).Count;

			return new Regex(@"\n", RegexOptions.Multiline).Matches(Текст.Substring(0, Index)).Count;

            //return Текст.Substring(0, Index).   Split(new char[] { '\n' }, StringSplitOptions.None).                Count();
        }

        public Модуль ДобавитьМетод(СвойстваМетодов Метод) {
            if (СловарьМетодов.ContainsKey(Метод.ИмяМетода)) ДобавитьПроблему("Метод "+Метод.ИмяМетода+" описан дважды", Метод.Index); else СловарьМетодов.Add(Метод.ИмяМетода, Метод);
            return this;
        }

        public void ПосчитатьВнутренниеВызовыМетодов(){
            foreach (var Метод in СловарьМетодов) { 
            //Метод.Метод.Key
            
            }
        }

        /// <summary>
        /// Метод производит поиск всех всех функции И процедур.
        /// отмечая их свойства: с запросом и стек вызовов
        /// </summary>
        /// <param name="МодульОбъекта">Модуль объекта.</param>
        private void НайтиВсеФункцииИПроцедуры()
        {
            var ПоискФункций = new Regex(@"^(?!\/\/)[^\.\/]*?(procedur|functio|Процедур|Функци)[enая][\s]*?([А-Яа-яa-z0-9_]*?)[\s]?\(([\S\s]*?)\)[\s]*?(экспорт|export)?([\S\s]*?)(Конец|End)\1[enыи]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection Найдены = ПоискФункций.Matches(Текст);
            foreach (Match Функция in Найдены)
            {
                СвойстваМетодов СвойствоМетода = new СвойстваМетодов(Функция);
                СвойствоМетода.НомерСтроки = ПолучитьНомерСтрокиПоИндексу(Функция.Index);
                ДобавитьМетод(СвойствоМетода);

            }

        }


    }


}
