using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using _1SCodeAnalyze.Структуры;

namespace _1SCodeAnalyze
{
    class АнализаторКода1С
    {

        Dictionary<String, Модуль> Модули;
        List<FileInfo> files;

        public АнализаторКода1С(List<FileInfo> files)
        {
            this.files = files;
            Модули = new Dictionary<string, Модуль>();
            ОбойтиВсеФайлы();

        }

        private void ОбойтиВсеФайлы()
        {
            foreach (FileInfo Файл in files)
            {
                String ИмяМодуля = Файл.Name.Replace(".Модуль.txt", "").Replace(".txt", "");
                Модуль МодульОбъекта = new Модуль(Файл);
                if (!Модули.ContainsKey(ИмяМодуля))
                {
                    Модули.Add(ИмяМодуля, МодульОбъекта);
                }
            }
            Console.WriteLine("Будет проанализировано "+Модули.Count.ToString()+" текстов Модулей");
			//foreach (KeyValuePair<String, Модуль> Объект in Модули)НайтиВсеФункцииИПроцедуры(Объект.Value);
            int КоличествоМодулей = Модули.Count;
			foreach (KeyValuePair<String, Модуль> Объект in Модули){
                АнализироватьЦиклы(Объект.Value);
                if (Объект.Value.ЕстьОшибки)
                {
                    Console.Write((int)(100 - ((float)КоличествоМодулей / (float)Модули.Count)*100.0f));
                    Console.WriteLine("%  " + Объект.Key );
                    foreach (var T in Объект.Value.ТаблицаАнализа)
                    {
						Console.WriteLine("строка "+Объект.Value.ПолучитьНомерСтрокиПоИндексу(T.Смещение) + ": \n" + T.ОписаниеПроблемы+ "\n");
                    }
                }
                КоличествоМодулей--;
			}
        }

		#region Методы поиска процедур и функций 

        /// <summary>
        /// Функция производит поиск запросов в вызываемых методах текста
        /// </summary>
        /// <param name="Текст"></param>
        /// <param name="МодульОбъекта"></param>
        /// <param name="Index"></param>
        /// <param name="СвойствоМетода"></param>
        /// <param name="ВызывающийМетод"></param>
        /// <returns></returns>
        private Boolean РекурсивныйПоискЗапроса(String Текст, Модуль МодульОбъекта, int Index,  СвойстваМетодов СвойствоМетода, String ВызывающийМетод, int Глубина)
        {
            ТелоКода Тело = new ТелоКода(Текст, Index);
			if (Тело.ЕстьЗапрос()) {
                СвойствоМетода.ЕстьЗапрос = true;
                if (Глубина == 0)
                {
                    МодульОбъекта.ДобавитьПроблему(Тело.ПолучитьАнализ());
                }
                else
                {
                    return true;
                }
            }
            //Ищем другие вызываемые процедуры
            MatchCollection Найдены = Тело.НайтиВызовы();
            foreach (Match Вызов in Найдены)
            {
				if(ВызывающийМетод.Contains(Вызов.Groups[1].Value))continue;//self call
				var ПоискМетода = new Regex(@"(Процедур|Функци|procedur|functio)[аяne][\s]*?" + ЭкранироватьРег(Вызов.Groups[1].Value) + @"[\s]*?\(([\S\s]*?)(Конец|end)\1[ыиen]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Match Найден = ПоискМетода.Match(МодульОбъекта.Текст);
                if (!Найден.Success)
                    continue;
                //натравим на эти процедуры эту же функцию
				if(РекурсивныйПоискЗапроса(Найден.Groups[2].Value, МодульОбъекта, Найден.Index, СвойствоМетода, Вызов.Groups[1].Value, Глубина + 1)){ 
					СвойствоМетода.ЕстьЗапрос = true;
					СвойствоМетода.ДобавитьВызов(Вызов.Groups[1].Value);
                    if (Глубина == 0) {
                        String СтекСтрокой = СвойствоМетода.ПолучитьСтекСтрокой()+"Запрос()";
						String КусокКода = (Текст.Length > 20 ? Текст.Substring(0, 20).Trim() : "") + "\n...\n" + СтекСтрокой;
						ИнформацияАнализа Анализ = new ИнформацияАнализа(Вызов.Groups[1].Index+Index,  КусокКода, СтекСтрокой);
                        МодульОбъекта.ДобавитьПроблему(Анализ);
						СвойствоМетода = new СвойстваМетодов();
                        continue; }
					return true;}
            }
            return false;
        }
        /// <summary>
        /// Фунция экранирует символы для нормальной подстановки в регулярное выражение
        /// </summary>
        /// <param name="s">Исходная строка</param>
        /// <returns>Результирующая строка</returns>
        private string ЭкранироватьРег(string s)
        {
            string p = s;
            foreach (char c in @"\|.+*(){}^$[]?/".ToCharArray())p = p.Replace("" + c, @"\" + c);
            return p.Replace(" ", @"[\s]*?");
        }

		/// <summary>
		/// Метод производит поиск всех всех функции И процедур.
		/// отмечая их свойства: с запросом и стек вызовов
		/// </summary>
		/// <param name="МодульОбъекта">Модуль объекта.</param>
		private void НайтиВсеФункцииИПроцедуры(Модуль МодульОбъекта)
        {
            var ПоискФункций = new Regex(@"^(?!\/\/)[^\.\/]*?(procedur|functio|Процедур|Функци)[enая][\s]*?([А-Яа-яa-z0-9_]*?)[\s]?\(([\S\s]*?)(Конец|End)\1[enыи]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			MatchCollection Найдены = ПоискФункций.Matches(МодульОбъекта.Текст);
            foreach (Match Функция in Найдены)
            {
				СвойстваМетодов СвойствоМетода = new СвойстваМетодов();
				СвойствоМетода.ЕстьЗапрос = РекурсивныйПоискЗапроса(Функция.Groups[3].Value, МодульОбъекта, Функция.Index, СвойствоМетода, Функция.Groups[2].Value , 1);
				СвойствоМетода.Index = Функция.Index;
				МодульОбъекта.ДобавитьМетод(Функция.Groups[2].Value, СвойствоМетода);
            }
        }
		#endregion

		public void АнализироватьЦиклы(Модуль МодульОбъекта)
        {

            var ПоискФункций = new Regex(@"(Для|Пока|for|while).+(Цикл|each|do)[\S\s]*?(КонецЦикла|endfor)", RegexOptions.IgnoreCase | RegexOptions.Multiline); //(Для|Пока).+ нужен иначе выражение находит ....Цикла;   код код код для цикл   КонецЦикла;
            //  необходимо переработать это выражение т.к если попадаются вложенные циклы то обрабатываются неверно
            MatchCollection Найдены = ПоискФункций.Matches(МодульОбъекта.Текст);
            foreach (Match Функция in Найдены)           
                РекурсивныйПоискЗапроса(Функция.Value, МодульОбъекта, Функция.Index, new СвойстваМетодов(), "", 0);
        }
    }
}

