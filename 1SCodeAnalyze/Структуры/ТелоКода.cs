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
        private int Index;
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
            var ПоискЗапроса = new Regex(@"^(?!\/\/)[^\/]*?\.(выполнить|найтипокоду|найтипореквизиту|найтипонаименованию)[\s]?\(", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Match Найдены = ПоискЗапроса.Match(Текст);
            if (!Найдены.Success)
                return null;
            return new ИнформацияАнализа(Найдены.Index + inIndex, Найдены.Groups[1].Value, Найдены.Groups[1].Value);
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


    }
}
