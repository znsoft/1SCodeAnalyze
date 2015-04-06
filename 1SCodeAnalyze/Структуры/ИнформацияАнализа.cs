using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _1SCodeAnalyze
{
	/// <summary>
	/// Информация анализа. хранение информации об анализе кода модуля
	/// </summary>
    class ИнформацияАнализа
    {
        public String ОписаниеПроблемы;
        public String ПроблемныйКод;
        public int Смещение;

		public ИнформацияАнализа(int index, String Описание, String Проблема){
			Смещение = index;
			ОписаниеПроблемы = Описание;
			ПроблемныйКод = Проблема;
		}
    }
	/// <summary>
	/// Свойства методов. класс хранящий свойства функций и процедур модуля
	/// </summary>
	class СвойстваМетодов{
		public Boolean Экспортный;
		public Boolean ЕстьЗапрос;
		public List<String> СтекВызовов;
		public int Index;

		public СвойстваМетодов(){
			СтекВызовов = new List<string>();
		}

		public СвойстваМетодов(Boolean q, Boolean e ){
			ЕстьЗапрос = q;
			Экспортный = e;
			СтекВызовов = new List<string>();
		}

		public void ДобавитьВызов (string value)
		{
			СтекВызовов.Add(value);
		}

        /// <summary>
        /// Получает строковое представление стека вызовов метода в виде Метод1()->Метод2()
        /// </summary>
        /// <returns></returns>
		public string ПолучитьСтекСтрокой ()
		{
			String s = "";
			foreach(String m in СтекВызовов)s = m + "()->"+s;
			if(String.IsNullOrEmpty(s)&&ЕстьЗапрос)s = "Запрос()";
			return s;
		}
	}
}
