using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace _1SCodeAnalyze.Структуры
{
    /// <summary>
    /// Свойства методов. класс хранящий свойства функций и процедур модуля
    /// </summary>
    class СвойстваМетодов
    {
        public Boolean Экспортный;
        public Boolean ЕстьЗапрос;
        public List<String> СтекВызовов;
        public int Index;
        public string ИмяМетода;
        public int НомерСтроки;
        public bool Экспорт;
        public bool РекурсивныйМетод;
        public int КоличествоИспользований;
        public ТелоКода ТелоМетода;
        public Dictionary<String, int> ВызываемыеМетоды;

        public СвойстваМетодов()
        {
            СтекВызовов = new List<string>();
            ВызываемыеМетоды = new Dictionary<String, int>();
        }

        public СвойстваМетодов(Boolean q, Boolean e)
        {
            СтекВызовов = new List<string>();
            ВызываемыеМетоды = new Dictionary<String, int>();

            ЕстьЗапрос = q;
            Экспортный = e;
        }

        public СвойстваМетодов(Match Функция)
        {
            СтекВызовов = new List<string>();
            ВызываемыеМетоды = new Dictionary<String, int>();
			this.Index = Функция.Index;
            this.ИмяМетода = Функция.Groups[2].Value.ToUpper();
            Экспорт = !String.IsNullOrEmpty(Функция.Groups[4].Value);
			this.Экспортный = Экспорт;
            ТелоМетода = (new ТелоКода(Функция.Groups[5].Value, Index)).ПровестиАнализ();
			this.ЕстьЗапрос = ТелоМетода.ЕстьЗапрос();
            ПосчитатьВнутренниеВызовыМетодов();

        }

        public СвойстваМетодов ДобавитьВызов(string value)
        {
            СтекВызовов.Add(value);
            return this;
        }


        void ПосчитатьВнутренниеВызовыМетодов(){
           MatchCollection Найдены = ТелоМетода.НайтиВызовы();
           foreach (Match Вызов in Найдены)
           {
               String ИмяВызова = Вызов.Groups[1].Value.ToUpper();
               if (ВызываемыеМетоды.ContainsKey(ИмяВызова)) ВызываемыеМетоды[ИмяВызова]++; else ВызываемыеМетоды.Add(ИмяВызова, 1);
           }
        }
        /// <summary>
        /// Получает строковое представление стека вызовов метода в виде Метод1()->Метод2()
        /// </summary>
        /// <returns></returns>
        public string ПолучитьСтекСтрокой()
        {
            String s = "";
            foreach (String m in СтекВызовов) s = m + "()->" + s;
            if (String.IsNullOrEmpty(s) && ЕстьЗапрос) s = "Запрос()";
            return s;
        }

        internal СвойстваМетодов УдалитьВызов(string p)
        {
            СтекВызовов.Remove(p);
            return this;
        }

        
    }

}
