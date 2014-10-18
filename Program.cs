using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace SimpleCalc
{
    class Program
    {
        static public bool GetInputString(out string inputString)
        {
            //string inputString = "";
            byte countBracketsStart = 0;
            byte countBracketsEnd = 0;
            const string allowedSymbols = "0123456789+-*/()";

            Console.WriteLine("Введите строку для вычисления:");
            inputString = Console.ReadLine();

            inputString = inputString.Trim();
            if (inputString.Length == 0)
                return false;


            if (inputString.IndexOf('=') >= 0)
                // если нахожу в строке "=" удаляю его и все, что справа
                inputString = inputString.Substring(0, inputString.IndexOf('='));

            // Проверяю количество скобок. Кол-во открывающих должно равняться количеству закрывающих

            for (int i = 0; i < inputString.Length; i++)
            {
                if (inputString[i] == '(')
                    countBracketsStart++;
                else if (inputString[i] == ')')
                    countBracketsEnd++;

                // Проверяю наличие ошибочных символов, если они присутствуют возвращаю из метода FALSE.
                if (!allowedSymbols.Contains(inputString[i]))
                    return false;

                // Начинаться и оканчиваться строка может только на цифру или скобку, в других случаях ошибка
                if (!IsCharsDigitsOrBrackets(inputString[0], inputString[inputString.Length - 1]))
                    return false;
            }
            // Если количество открывающих не равно кол-ву закрываемых, возвращаю признак неуспешности.
            if (countBracketsStart != countBracketsEnd)
                return false;
            return true;
        }


        static int DoSomeCalculation(string inputString)
        {
            string[] strItems;

            List<char> oprList = new List<char>();
            List<int> numbList = new List<int>();

            char[] splitChars = new char[4] { '+', '-', '*', '/' };
            // Если во входящей строке есть 
            while (inputString.Contains('('))
            {
                // Если выражение содержит скобки, копирую выражение которое входит во внешние скобки,
                // и передаю рекурсивно на вход этого же метода. Рекурсия будет продолжаться до тех пор пока
                // не раскроются все скобки. После этого рекурсивный метод возвратит численное значение (результат) 
                // вычисления выражения во всех скобках. Подставляю этот результат во входную строку вместо выражения в скобках.



                int tmpInt;
                string tmpStr;
                string strInBrackets;
                ushort indexOfStartBracket, indexOfEndBracket;
                ushort tmpBracketsCount = 1;

                // Нахожу первую группу скобок возможно вложенных.
                indexOfStartBracket = (ushort)inputString.IndexOf('(');
                indexOfEndBracket = (ushort)inputString.LastIndexOf(')');
                for (int i = indexOfStartBracket + 1; i < inputString.Length; i++)
                {
                    if (inputString[i] == '(')
                        tmpBracketsCount++;
                    if (inputString[i] == ')')
                        tmpBracketsCount--;
                    if (tmpBracketsCount == 0 && inputString[i] == ')')
                    {
                        indexOfEndBracket = (ushort)i;
                        break;
                    }
                }

                strInBrackets = inputString.Substring(indexOfStartBracket + 1, indexOfEndBracket - indexOfStartBracket - 1);
                // Выражение в скобках должно начинаться и оканчиваться на скобку или цифру, 
                // если по другому- ошибка, генерирую исключение, также производится проверка на то, чтобы длинна передаваемой строки
                // не равнялась 0.
                if (indexOfStartBracket == indexOfEndBracket + 1 ||
                    (!IsCharsDigitsOrBrackets((char)strInBrackets[0], (char)strInBrackets[strInBrackets.Length - 1])))
                    throw new Exception();
                // Вызываю метод рекурсивно с новой строкой.
                tmpInt = DoSomeCalculation(strInBrackets);
                tmpStr = inputString.Substring(0, indexOfStartBracket) + tmpInt.ToString() + inputString.Substring(indexOfEndBracket + 1, inputString.Length - 1 - indexOfEndBracket);
                inputString = tmpStr;
            }


            // На данном этапе входная строка содержит только линейные выражения без скобок, т.е. операнды и 
            // арифметические операторы.
            // Разбиваю сторку на подстроки, преобразовываю и заношу числа в numbList 
            strItems = inputString.Split(splitChars);
            for (int i = 0; i < strItems.GetLength(0); i++)
                numbList.Add(Convert.ToInt32(strItems[i]));

            // Выбираю все арифметические операторы из входноый строки в порядке следования и заношу в oprList.
            for (int i = 0; i < inputString.Length; i++)
                if (inputString[i] == '+' || inputString[i] == '-' || inputString[i] == '*' || inputString[i] == '/')
                    oprList.Add(Convert.ToChar(inputString[i]));

            // Если в списке арифметических операторов есть "*" "/", эти расчеты выполняем в первую очередь. 
            while (oprList.Contains('*') || oprList.Contains('/'))
            {
                int indexCurrOpr = -1;
                int tmpRes;

                // Нахожу минимальное значение индекса для операторов '/' или '*'.
                if (oprList.Contains('*') && oprList.Contains('/'))
                    indexCurrOpr = (oprList.IndexOf('*') < oprList.IndexOf('/')) ? oprList.IndexOf('*') : oprList.IndexOf('/');
                else
                    if (oprList.Contains('*'))
                        indexCurrOpr = oprList.IndexOf('*');
                    else if ((oprList.Contains('/')))
                        indexCurrOpr = oprList.IndexOf('/');

                // Вычисляю выражение для самого старшего оператора, и записываю результат в numbList
                // на место первого операнда, а второй операнд удаляю из numbList. Также удаляю арифметический оператор
                // из oprList, действия для которого уже выполнили.
                tmpRes = DoArifmeticActions(oprList[indexCurrOpr], numbList[indexCurrOpr], numbList[indexCurrOpr + 1]);
                oprList.RemoveAt(indexCurrOpr);
                numbList.RemoveAt(indexCurrOpr);
                numbList[indexCurrOpr] = tmpRes;
                //              Console.WriteLine(numbList[indexCurrOpr]);
            }

            // Если в списке арифметических операторов есть "+" "-", эти расчеты выполняем во вторую очередь. 
            while (oprList.Contains('-') || oprList.Contains('+'))
            {
                int indexCurrOpr = -1;
                int tmpRes;

                // Нахожу минимальное значение индекса для операторов '-' или '+'.
                if (oprList.Contains('-') && oprList.Contains('+'))
                    indexCurrOpr = (oprList.IndexOf('-') < oprList.IndexOf('+')) ? oprList.IndexOf('-') : oprList.IndexOf('+');
                else
                    if (oprList.Contains('+'))
                        indexCurrOpr = oprList.IndexOf('+');
                    else if (oprList.Contains('-'))
                        indexCurrOpr = oprList.IndexOf('-');

                // Вычисляю выражение для самого старшего арифметического оператора, и записываю результат в numbList
                // на место первого операнда, а второй операнд удаляю из numbList. Также удаляю арифметический оператор
                // из oprList, действия для которого уже выполнили.
                tmpRes = DoArifmeticActions(oprList[indexCurrOpr], numbList[indexCurrOpr], numbList[indexCurrOpr + 1]);
                oprList.RemoveAt(indexCurrOpr);
                numbList.RemoveAt(indexCurrOpr);
                numbList[indexCurrOpr] = tmpRes;
                //                Console.WriteLine(numbList[indexCurrOpr]);
            }

            return numbList[0];
        }


        // Выполняет элементарные арифметические действия и возвращает результат
        static int DoArifmeticActions(char opr, int leftOperand, int rightOperand)
        {
            int result;
            if (opr == '+')
                result = leftOperand + rightOperand;
            else if (opr == '-')
                result = leftOperand - rightOperand;
            else if (opr == '/')
                result = leftOperand / rightOperand;
            else if (opr == '*')
                result = leftOperand * rightOperand;
            else
                result = 0;


            return result;
        }


        // Возвращает сообщение об ошибке, и завершает программу.
        static void ShowErrorMessageAndExit()
        {
            Console.WriteLine("Ошибочное выражение!");
            Console.WriteLine("Для продолжения нажмите любую клавишу!");
            Console.ReadKey();
            Environment.Exit(-1);

        }

        // Этот метод проверяет является ли переданные символы открывающими или закрывающими скобками или 
        // числами. Если да, возвращает True.
        static bool IsCharsDigitsOrBrackets(char firstChar, char lastChar)
        {
            if (firstChar != '(' && ((int)firstChar < 48 || (int)firstChar > 57) ||
                lastChar != ')' && ((int)lastChar < 48 || (int)lastChar > 57))
                return false;
            else
                return true;
        }


        static void Main(string[] args)
        {
            string inputString;
            // GetInputString(out inputString) возвращает true или false в зависимости от успешности
            // получения входной строки, и через *out* возвращает полученную строку.
            if (!GetInputString(out inputString))
                ShowErrorMessageAndExit();

            // DoSomeCalculation(inputString); возвращает интовое значение результата, 
            // и в случае неправильного расставления скобок или арифметических операторов
            // сгенерирует исключение.
            try
            {
                Console.WriteLine("\nРезультат:\n{0}={1}", inputString, DoSomeCalculation(inputString));
                Console.WriteLine("Для продолжения нажмите любую клавишу!");
                Console.ReadKey();
            }
            catch
            {
                ShowErrorMessageAndExit();
            }

        }
    }
}
