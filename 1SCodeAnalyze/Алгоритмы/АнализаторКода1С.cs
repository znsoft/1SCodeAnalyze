﻿using System;
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
        StreamWriter sw_fileReport;

        public АнализаторКода1С(List<FileInfo> files)
        {
            this.files = files;
            Модули = new Dictionary<string, Модуль>();
            //добавил создание файла отчета, потому что в консоли неудобно читать логи на больших конфах
            string fileNameReport = Directory.GetCurrentDirectory() + "\\report_" + DateTime.Now.ToString().Replace(".", "").Replace(":", "").Replace(" ", "") + ".txt";
            FileInfo file = new FileInfo(fileNameReport);

            sw_fileReport = File.CreateText(fileNameReport);
            ОбойтиВсеФайлы();
            sw_fileReport.Close();

        }

        public void AddLog(string message)
        {
            Console.WriteLine(message);
            sw_fileReport.WriteLine(message);
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
            AddLog("Будет проанализировано " + Модули.Count.ToString() + " текстов Модулей");
            //foreach (KeyValuePair<String, Модуль> Объект in Модули)НайтиВсеФункцииИПроцедуры(Объект.Value);
            int КоличествоМодулей = Модули.Count;
            foreach (KeyValuePair<String, Модуль> Объект in Модули)
            {
                АнализироватьЦиклы(Объект.Value);
                if (Объект.Value.ЕстьОшибки)
                {
                    string tmp = ((int)(100 - ((float)КоличествоМодулей / (float)Модули.Count) * 100.0f)).ToString();
                    tmp = tmp + "%  " + Объект.Key + Environment.NewLine;
                    AddLog(tmp);
                    foreach (var T in Объект.Value.ТаблицаАнализа)
                    {
                        tmp = "строка " + Объект.Value.ПолучитьНомерСтрокиПоИндексу(T.Смещение) + ": \n" + T.ОписаниеПроблемы + "\n";
                        Console.WriteLine(tmp);
                        AddLog(tmp);
                    }
                }
                КоличествоМодулей--;
            }
        }

        #region Методы поиска процедур и функций

        /// <summary>
        /// Функция производит поиск запросов в вызываемых методах текста
        /// </summary>
        /// <param name="Текст">Текст процедуры</param>
        /// <param name="МодульОбъекта">Весь файл</param>
        /// <param name="Index">точка входа в процедуру </param>
        /// <param name="СвойствоМетода">Свойства вызываемых процедур</param>
        /// <param name="ВызывающийМетод">защита от рекурсий</param>
        /// <returns>Истина/Ложь</returns>
        private Boolean РекурсивныйПоискЗапроса(String Текст, Модуль МодульОбъекта, int Index, СвойстваМетодов СвойствоМетода, String ВызывающийМетод, int Глубина)
        {
            if (Глубина > 20)
            {
                String СтекСтрокой = СвойствоМетода.ПолучитьСтекСтрокой();
                String КусокКода = "Запутаная рекурсия в  " + МодульОбъекта.file.Name + " Методе " + ВызывающийМетод + " -> " + СтекСтрокой + "\nМетод вызывает цепочку методов которые снова вызывают этот же метод, возможно переполнение стека";

                ИнформацияАнализа Анализ = new ИнформацияАнализа(Index, КусокКода, СтекСтрокой);
                МодульОбъекта.ДобавитьПроблему(Анализ);
                СвойствоМетода = new СвойстваМетодов();
                return false;
            }
            ТелоКода Тело = new ТелоКода(Текст, Index);
            if (Тело.ЕстьЗапрос())
            {
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
                if (ВызывающийМетод.ToUpper().Contains(Вызов.Groups[1].Value.ToUpper())) continue;//self call //рекурсия
                var ПоискМетода = new Regex(@"(Процедур|Функци|procedur|functio)[аяne][\s]*?" + ЭкранироватьРег(Вызов.Groups[1].Value) + @"[\s]*?\(([\S\s]*?)(Конец|end)\1[ыиen]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Match Найден = ПоискМетода.Match(МодульОбъекта.Текст);
                if (!Найден.Success)
                    continue;
                //натравим на эти процедуры эту же функцию
                СвойствоМетода.ДобавитьВызов(Вызов.Groups[1].Value);
                if (РекурсивныйПоискЗапроса(Найден.Groups[2].Value, МодульОбъекта, Найден.Index, СвойствоМетода, Вызов.Groups[1].Value, Глубина + 1))
                {
                    СвойствоМетода.ЕстьЗапрос = true;

                    if (Глубина == 0)
                    {
                        String СтекСтрокой = СвойствоМетода.ПолучитьСтекСтрокой() + "Запрос()";
                        String КусокКода = (Текст.Length > 20 ? Текст.Substring(0, 20).Trim() : "") + "\n...\n" + СтекСтрокой;
                        ИнформацияАнализа Анализ = new ИнформацияАнализа(Вызов.Groups[1].Index + Index, КусокКода, СтекСтрокой);
                        МодульОбъекта.ДобавитьПроблему(Анализ);
                        СвойствоМетода = new СвойстваМетодов();
                        continue;
                    }
                    return true;
                }
                else
                {
                    СвойствоМетода.УдалитьВызов(Вызов.Groups[1].Value);
                }
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
            foreach (char c in @"\|.+*(){}^$[]?/".ToCharArray()) p = p.Replace("" + c, @"\" + c);
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
                СвойствоМетода.ЕстьЗапрос = РекурсивныйПоискЗапроса(Функция.Groups[3].Value, МодульОбъекта, Функция.Index, СвойствоМетода, Функция.Groups[2].Value, 1);
                СвойствоМетода.Index = Функция.Index;
                //МодульОбъекта.ДобавитьМетод(Функция.Groups[2].Value, СвойствоМетода);
            }
        }
        #endregion

        public void АнализироватьЦиклы(Модуль МодульОбъекта)
        {

            var ПоискФункций = new Regex(@"(Для|Пока|for|while).+(Цикл|do)[\S\s]*?(КонецЦикла|endfor|enddo)", RegexOptions.IgnoreCase | RegexOptions.Multiline); //(Для|Пока).+ нужен иначе выражение находит ....Цикла;   код код код для цикл   КонецЦикла;
            //  необходимо переработать это выражение т.к если попадаются вложенные циклы то обрабатываются неверно
            MatchCollection Найдены = ПоискФункций.Matches(МодульОбъекта.Текст);
            foreach (Match Функция in Найдены)
                РекурсивныйПоискЗапроса(Функция.Value, МодульОбъекта, Функция.Index, new СвойстваМетодов(), "", 0);
        }
    }
}

