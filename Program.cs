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
            byte countBracketsStart = 0;
            byte countBracketsEnd = 0;
            const string allowedSymbols = "0123456789+-*/()";

            Console.WriteLine("Введите строку для вычисления:");
            inputString = Console.ReadLine();

            inputString = inputString.Trim();
            if (inputString.Length == 0)
                return false;


            if (inputString.IndexOf('=') >= 0)
                // если нахожу в строке "=" удаляю его, и все что справа
                inputString = inputString.Substring(0, inputString.IndexOf('='));

            // Начинаться и оканчиваться строка может только на цифру знак минус или скобку, в других случаях ошибка
            if (!IsCharsDigitsOrBrackets(inputString[0], inputString[inputString.Length - 1]))
                return false;

            // Проверяю количество скобок. Кол-во открывающих должно равняться количеству закрывающих
            for (int i = 0; i < inputString.Length; i++)
            {
                if (inputString[i] == '(')
                    countBracketsStart++;
                else if (inputString[i] == ')')
                    countBracketsEnd++;
                if (countBracketsStart < countBracketsEnd)
                    return false; // Возвращаю ошибку, т.к. количество открываемых скобок на любой из итераций
                // должно быть больше или равно количеству закрываемых.

                // Проверяю наличие не разрешенных символов, если они присутствуют возвращаю ошибку.
                if (!allowedSymbols.Contains(inputString[i]))
                    return false;
            }
            // Если количество открывающих не равно кол-ву закрываемых, возвращаю признак неуспешности.
            if (countBracketsStart != countBracketsEnd)
                return false;
            return true;
        }

        static double DoSomeCalculation(string inputString)
        {
            while (inputString.Contains('('))
            {
                // Для начала считаю выражение в скобках.
                double tmpDbl;
                string strInBrackets;
                ushort indexOfStartBracket, indexOfEndBracket, tmpBracketsCount = 1;

                // Нахожу границы первой группы скобок.
                indexOfStartBracket = (ushort)inputString.IndexOf('(');
                indexOfEndBracket = (ushort)inputString.LastIndexOf(')');
                for (int i = indexOfStartBracket + 1; i < inputString.Length; i++)
                {
                    if (inputString[i] == '(')
                        tmpBracketsCount++;
                    else if (inputString[i] == ')')
                        tmpBracketsCount--;
                    if (tmpBracketsCount == 0 && inputString[i] == ')')
                    {
                        indexOfEndBracket = (ushort)i;// Заношу позицию закрыавающей скобки в переменную.
                        break;
                    }
                }

                // Зная индексы открывающей и закрывающей скобок, копирую выражение в скобках в новую строковую переменную.
                strInBrackets = inputString.Substring(indexOfStartBracket + 1, indexOfEndBracket - indexOfStartBracket - 1);

                // Проверка: выражение в скобках должно начинаться и оканчиваться на скобку или цифру.
                // Также производится проверка на то, чтобы длинна передаваемой строки не равнялась 0.
                // Если по другому - ошибка, генерирую исключение.
                if (indexOfStartBracket + 1 == indexOfEndBracket ||
                    (!IsCharsDigitsOrBrackets((char)strInBrackets[0], (char)strInBrackets[strInBrackets.Length - 1])))
                    throw new Exception();

                // Рекурсивно вызываю этот же метод но в качестве аргумента передаю подстроку, 
                // которую содержит первая группа скобок. Подстрока может также содержать вложенные скобки.
                // Пример: входная строка была "5*((10-3)*2-4)+10", в метод передаю "(10-3)*2-4"
                tmpDbl = DoSomeCalculation(strInBrackets);

                // Полученный результат типа Double преобразовываю в ToString и подставляю в исходную строку вместо скобок
                // и tmpStr будет содержать строку уже без скобок. Для вышеприведенного примера строка получится вида "5*10+10".

                inputString = inputString.Substring(0, indexOfStartBracket) + tmpDbl.ToString() + inputString.Substring(indexOfEndBracket + 1, inputString.Length - 1 - indexOfEndBracket);
            }

            // Выполняю вычисление выражения которое уже не содержит скобок, и возвращаю результат.
            return DoCalculationWithoutBrackets(inputString);
        }

        static double DoCalculationWithoutBrackets(string inputString)
        {
            // На данном этапе входная строка содержит только линейные выражения без скобок, т.е. операнды и арифметические операторы.
            List<char> oprList = new List<char>();
            List<double> numbList = new List<double>();
            char[] splitChars = new char[4] { '+', '-', '*', '/' };

            // Разбиваю строку на числа
            string[] strItems = inputString.Split(splitChars);


            // Конвертирую подстроки в числа и заношу их в список
            for (int i = 0; i < strItems.Length; i++)
            {
                try
                {
                    numbList.Add(Convert.ToDouble(strItems[i]));
                }
                catch { }
            }
            // Если в метод передана строка начинающаяся на '-', умножаю на -1 соответствующий операнд.
            if (inputString[0] == '-')
            {
                numbList[0] *= -1;
                inputString= inputString.Remove(0, 1);//TODO: Нужна оптимизация.
            }

            // Выбираю все арифметические операторы из входной строки в порядке следования и заношу в oprList.
            for (int i = 0; i < inputString.Length; i++)
                if (inputString[i] == '+' || inputString[i] == '-' || inputString[i] == '*' || inputString[i] == '/')
                {
                    oprList.Add(Convert.ToChar(inputString[i]));
                    if (i < inputString.Length - 1 && (inputString[i + 1] == '-'))
                    {
                        numbList[oprList.Count] *= -1;
                        i++;// пропускю минус, который относится к знаку числа, а не арифметической операции.
                    }
                }

            // Если в списке арифметических операторов есть "*" "/", эти расчеты выполняем в первую очередь. 
            while (oprList.Contains('*') || oprList.Contains('/'))
            {
                int indexCurrOpr = -1;
                //  double tmpRes;

                // Определяю какой из операторов идет раньше '/' или '*'.
                if (oprList.Contains('*') && oprList.Contains('/'))
                    indexCurrOpr = (oprList.IndexOf('*') < oprList.IndexOf('/')) ? oprList.IndexOf('*') : oprList.IndexOf('/');
                else
                    if (oprList.Contains('*'))
                        indexCurrOpr = oprList.IndexOf('*');
                    else if ((oprList.Contains('/')))
                        indexCurrOpr = oprList.IndexOf('/');

                // Вычисляю выражение для самого старшего оператора, и записываю результат в numbList на место первого операнда,
                //  а второй операнд удаляю из numbList. Также удаляю арифметический оператор из oprList, действия для которого уже выполнили.
                numbList[indexCurrOpr] = DoArifmeticActions(oprList[indexCurrOpr], numbList[indexCurrOpr], numbList[indexCurrOpr + 1]);
                oprList.RemoveAt(indexCurrOpr);
                numbList.RemoveAt(indexCurrOpr + 1);
            }

            // Если в списке арифметических операторов есть "+" "-", эти расчеты выполняем во вторую очередь. 
            while ((oprList.Contains('-')) || oprList.Contains('+'))
            {
                int indexCurrOpr = -1;

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
                numbList[indexCurrOpr] = DoArifmeticActions(oprList[indexCurrOpr], numbList[indexCurrOpr], numbList[indexCurrOpr + 1]);
                oprList.RemoveAt(indexCurrOpr);
                numbList.RemoveAt(indexCurrOpr + 1);
                //numbList[indexCurrOpr] = tmpRes;
            }
            return numbList[0];
        }

        // Выполняет элементарные арифметические действия и возвращает результат
        static double DoArifmeticActions(char opr, double leftOperand, double rightOperand)
        {
            double result = 0;
            switch (opr)
            {
                case '+':
                    result = leftOperand + rightOperand;
                    break;
                case '-':
                    result = leftOperand - rightOperand;
                    break;
                case '/':
                    if (rightOperand == 0)
                        throw new System.DivideByZeroException();
                    result = leftOperand / rightOperand;
                    break;
                case '*':
                    result = leftOperand * rightOperand;
                    break;
                default:
                    result = 0;
                    break;
            }
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
        // числами, также может начинаться на знак '-'. Если да, возвращает True.
        static bool IsCharsDigitsOrBrackets(char firstChar, char lastChar)
        {
            if (firstChar != '-' && firstChar != '(' && ((int)firstChar < 48 || (int)firstChar > 57) ||
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

            // DoSomeCalculation(inputString); возвращает double значение результата.
            try
            {
                double result = DoSomeCalculation(inputString);
                Console.WriteLine("\nРезультат:\n{0}={1}", inputString, result);
                Console.WriteLine("Для продолжения нажмите любую клавишу!");
                Console.ReadKey();
            }
            catch (DivideByZeroException e)
            {
                Console.WriteLine("Ошибка, деление на 0!!!");
            }
            catch
            {
                ShowErrorMessageAndExit();
            }
        }
    }
}