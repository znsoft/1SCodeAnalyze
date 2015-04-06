using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace _1SCodeAnalyze.Структуры
{

	/// <summary>
	/// Тело кода. класс куда попадает и анализируется тело процедуры или цикла
	/// </summary>
    class ТелоКода
    {
        String Текст;
        Модуль МодульОбъекта;
        int inIndex;
        private Boolean АнализПрямогоЗапросаПроведен;
        private Boolean ЗапросЕсть;
        private ИнформацияАнализа Анализ;

        public ТелоКода(String Текст,  Модуль МодульОбъекта,     int inIndex) {
            ЗапросЕсть = false;
            this.inIndex = inIndex;
            this.Текст = Текст;
            this.МодульОбъекта = МодульОбъекта;
            АнализПрямогоЗапросаПроведен = false;
        }

        private ИнформацияАнализа ПрямойЗапрос()
        {
            var ПоискЗапроса = new Regex(@"^[^\/]*?\.(выполнить|найтипокоду|найтипореквизиту|найтипонаименованию)[\s]?\(.+$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Match Найдены = ПоискЗапроса.Match(Текст);
            if (!Найдены.Success)
                return null;
            return new ИнформацияАнализа(Найдены.Index + inIndex, " " + (Текст.Length > 120 ? Текст.Substring(Найдены.Index > 50 ? Найдены.Index - 50 : 0, 120) : Найдены.Value) , Найдены.Groups[1].Value);
            //    		Найдены.Value	"Пока В  Цикл\r\n\tыборка = Запрос.Выполнить().Выбрать();\r"	            return new ИнформацияАнализа(Найдены.Groups[1].Index + inIndex, "Запрос ... " + Текст.Substring(Найдены.Index, 60) + Найдены.Groups[1].Value, Найдены.Groups[1].Value);
        }

        public ТелоКода ПровестиАнализ()
        {
            if (АнализПрямогоЗапросаПроведен) return this;
            Анализ = ПрямойЗапрос();
            ЗапросЕсть = (Анализ != null);
            АнализПрямогоЗапросаПроведен = true;
            return this;
        }

        public ИнформацияАнализа ПолучитьАнализ(){
            ПровестиАнализ();
            return Анализ;
        }


        public Boolean ЕстьЗапрос()
        {
            ПровестиАнализ();
            return ЗапросЕсть;
        }

        public MatchCollection НайтиВызовы()
        {
            var ПоискВызовов = new Regex(@"^(?!\/\/)[^\.\/]*?([а-яa-z0-9_]*?)[\s]?\(", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return ПоискВызовов.Matches(Текст);
        }
    }
}
