/// <summary>
/// Перечисление, представляющее различные лексемы или токены, используемые в лексическом анализе.
/// </summary>
public enum Lexems
{
    None, Name,
    True, False, Boolean,
    Begin, End, Var, Print, Assign, If, While, EndIf, EndWhile,
    Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual,
    Then, ElseIf, Else, Do, LeftBracket, RightBracket, Semi,
    Comma, EOF, Colon, Remainder,
    Disjunction, Conjunction, Xor,
    Implication, Negation, BinaryOp,
    Sum, Subtract, Multiplication, Division,
    DotNot, IntNumber
}

// <summary>
// Структура, представляющая ключевое слово с его соответствующей лексемой.
// </summary>
public struct Keyword
{
    /// <summary>
    /// Ключевое слово
    /// </summary>
    public string word;

    /// <summary>
    /// Соответствующая ключевому слову лексема
    /// </summary>
    public Lexems lex;
}

/// <summary>
/// Статический класс, ответственный за лексический анализ исходного кода.
/// </summary>
public static class LexicalAnalyzer
{
    /// <summary>
    /// Массив ключевых слов
    /// </summary>
    private static Keyword[] keywords = new Keyword[30];

    /// <summary>
    /// Указатель для отслеживания добавленных ключевых слов
    /// </summary>
    private static int keywordsPointer = 0;

    /// <summary>
    /// Текущая лексема, которая анализируется
    /// </summary>
    private static Lexems currentLexem;

    /// <summary>
    /// Текущее имя идентификатора
    /// </summary>
    private static string? currentName;

    /// <summary>
    /// Максимальная длина названия идентификатора
    /// </summary>
    private const int MaxIdentifierLength = 50;

    /// <summary>
    /// Инициализирует лексический анализатор с указанным путем к файлу.
    /// </summary>
    /// <param name="code">Исходный файл.</param>
    public static void Initialize(string code)
    {
        keywords = new Keyword[30];
        keywordsPointer = 0;

        AddKeyword("Begin", Lexems.Begin);
        AddKeyword("End", Lexems.End);
        AddKeyword("Var", Lexems.Var);
        AddKeyword("Print", Lexems.Print);
        AddKeyword("Boolean", Lexems.Boolean);
        AddKeyword("True", Lexems.True);
        AddKeyword("False", Lexems.False);
        AddKeyword("If", Lexems.If);
        AddKeyword("While", Lexems.While);
        AddKeyword("EndIf", Lexems.EndIf);
        AddKeyword("EndWhile", Lexems.EndWhile);
        AddKeyword("Then", Lexems.Then);
        AddKeyword("ElseIf", Lexems.ElseIf);
        AddKeyword("Else", Lexems.Else);
        AddKeyword("Do", Lexems.Do);

        Reader.Initialize(code);
        currentLexem = Lexems.None;
        currentName = null;
    }

    /// <summary>
    /// Добавляет ключевое слово в список ключевых слов.
    /// </summary>
    /// <param name="keyword">Ключевое слово в виде строки.</param>
    /// <param name="lex">Связанная лексема.</param>
    private static void AddKeyword(string keyword, Lexems lex)
    {
        Keyword kw = new Keyword { word = keyword, lex = lex };
        keywords[keywordsPointer++] = kw;
    }

    /// <summary>
    /// Получает лексему, связанную с указанным ключевым словом.
    /// </summary>
    /// <param name="keyword">Ключевое слово для получения лексемы.</param>
    /// <returns>Соответствующая лексема.</returns>
    private static Lexems GetKeywordLexem(string keyword)
    {
        for (int i = 0; i < keywordsPointer; i++)
        {
            if (keywords[i].word == keyword)
                return keywords[i].lex;
        }
        return Lexems.Name;
    }

    /// <summary>
    /// Парсит следующую лексему в исходном коде.
    /// </summary>
    public static void ParseNextLexem()
    {
        // Пропускаем пробелы, табуляции, символы возврата каретки и новые строки
        while (char.IsWhiteSpace(Reader.CurrentSymbol) ||
               Reader.CurrentSymbol == '\r' ||
               Reader.CurrentSymbol == '\n')
        {
            Reader.ReadNextSymbol();
        }

        if (Reader.CurrentSymbol == Reader.EndOfFile)
        {
            currentLexem = Lexems.EOF;
            return;
        }

        if (char.IsLetter(Reader.CurrentSymbol))
        {
            ParseIdentifier();
        }
        else if (char.IsDigit(Reader.CurrentSymbol))
        {
            ParseInteger();
        }
        else if (Reader.CurrentSymbol == '(')
        {
            currentName = null;
            Reader.ReadNextSymbol();
            currentLexem = Lexems.LeftBracket;
        }
        else if (Reader.CurrentSymbol == ')')
        {
            currentName = null;
            Reader.ReadNextSymbol();
            currentLexem = Lexems.RightBracket;
        }
        else if (Reader.CurrentSymbol == ';')
        {
            currentName = null;
            Reader.ReadNextSymbol();
            currentLexem = Lexems.Semi;
        }
        else if (Reader.CurrentSymbol == ':')
        {
            Reader.ReadNextSymbol();
            if (Reader.CurrentSymbol == '=')
            {
                currentName = null;
                Reader.ReadNextSymbol();
                currentLexem = Lexems.Assign;
            }
            else
            {
                currentName = null;
                currentLexem = Lexems.Colon;
            }
        }
        else if (Reader.CurrentSymbol == ',')
        {
            currentName = null;
            Reader.ReadNextSymbol();
            currentLexem = Lexems.Comma;
        }
        else if (Reader.CurrentSymbol == '+')
        {
            currentName = null;
            currentLexem = Lexems.Sum;
            Reader.ReadNextSymbol();
        }
        else if (Reader.CurrentSymbol == '-')
        {
            currentName = null;
            currentLexem = Lexems.Subtract;
            Reader.ReadNextSymbol();
        }
        else if (Reader.CurrentSymbol == '*')
        {
            currentName = null;
            currentLexem = Lexems.Multiplication;
            Reader.ReadNextSymbol();
        }
        else if (Reader.CurrentSymbol == '/')
        {
            currentName = null;
            currentLexem = Lexems.Division;
            Reader.ReadNextSymbol();
        }
        else if (Reader.CurrentSymbol == '%')
        {
            currentName = null;
            currentLexem = Lexems.Remainder;
            Reader.ReadNextSymbol();
        }
        else if (Reader.CurrentSymbol == '.')
        {
            ParseDotOperator();
        }
        else
        {
            throw new Exception($"Ошибка: Недопустимый символ: '{Reader.CurrentSymbol}' (код: {(int)Reader.CurrentSymbol}) в строке {Reader.LineNumber}, позиция {Reader.CharacterPositionInLine}");
        }
    }

    private static void ParseDotOperator()
    {
        // Сохраняем позицию начала оператора
        int startLine = Reader.LineNumber;
        int startPos = Reader.CharacterPositionInLine;

        // Читаем символ после точки
        Reader.ReadNextSymbol();

        // Пропускаем пробелы после точки? НЕТ - оператор должен быть без пробелов
        // Если после точки пробел - это ошибка
        if (char.IsWhiteSpace(Reader.CurrentSymbol))
        {
            throw new Exception($"Ошибка: После точки ожидается оператор (NOT, AND, OR, XOR), а найден пробел в строке {Reader.LineNumber}, позиция {Reader.CharacterPositionInLine}");
        }

        string operatorName = "";

        // Читаем .NOT.
        if (Reader.CurrentSymbol == 'N' || Reader.CurrentSymbol == 'n')
        {
            operatorName = "." + Reader.CurrentSymbol;
            Reader.ReadNextSymbol();

            if (Reader.CurrentSymbol == 'O' || Reader.CurrentSymbol == 'o')
            {
                operatorName += Reader.CurrentSymbol;
                Reader.ReadNextSymbol();

                if (Reader.CurrentSymbol == 'T' || Reader.CurrentSymbol == 't')
                {
                    operatorName += Reader.CurrentSymbol;
                    Reader.ReadNextSymbol();

                    // Проверяем, что после T идет точка
                    if (Reader.CurrentSymbol == '.')
                    {
                        Reader.ReadNextSymbol();
                        currentName = null;
                        currentLexem = Lexems.DotNot;
                        return;
                    }
                    else
                    {
                        throw new Exception($"Ошибка: Ожидается '.' после .NOT, найден '{Reader.CurrentSymbol}' в строке {Reader.LineNumber}");
                    }
                }
            }
        }
        // Читаем .AND.
        else if (Reader.CurrentSymbol == 'A' || Reader.CurrentSymbol == 'a')
        {
            operatorName = "." + Reader.CurrentSymbol;
            Reader.ReadNextSymbol();

            if (Reader.CurrentSymbol == 'N' || Reader.CurrentSymbol == 'n')
            {
                operatorName += Reader.CurrentSymbol;
                Reader.ReadNextSymbol();

                if (Reader.CurrentSymbol == 'D' || Reader.CurrentSymbol == 'd')
                {
                    operatorName += Reader.CurrentSymbol;
                    Reader.ReadNextSymbol();

                    // Проверяем, что после D идет точка
                    if (Reader.CurrentSymbol == '.')
                    {
                        Reader.ReadNextSymbol();
                        currentName = null;
                        currentLexem = Lexems.Conjunction;
                        return;
                    }
                    else
                    {
                        throw new Exception($"Ошибка: Ожидается '.' после .AND, найден '{Reader.CurrentSymbol}' в строке {Reader.LineNumber}");
                    }
                }
            }
        }
        // Читаем .OR.
        else if (Reader.CurrentSymbol == 'O' || Reader.CurrentSymbol == 'o')
        {
            operatorName = "." + Reader.CurrentSymbol;
            Reader.ReadNextSymbol();

            if (Reader.CurrentSymbol == 'R' || Reader.CurrentSymbol == 'r')
            {
                operatorName += Reader.CurrentSymbol;
                Reader.ReadNextSymbol();

                // Проверяем, что после R идет точка
                if (Reader.CurrentSymbol == '.')
                {
                    Reader.ReadNextSymbol();
                    currentName = null;
                    currentLexem = Lexems.Disjunction;
                    return;
                }
                else
                {
                    throw new Exception($"Ошибка: Ожидается '.' после .OR, найден '{Reader.CurrentSymbol}' в строке {Reader.LineNumber}");
                }
            }
        }
        // Читаем .XOR.
        else if (Reader.CurrentSymbol == 'X' || Reader.CurrentSymbol == 'x')
        {
            operatorName = "." + Reader.CurrentSymbol;
            Reader.ReadNextSymbol();

            if (Reader.CurrentSymbol == 'O' || Reader.CurrentSymbol == 'o')
            {
                operatorName += Reader.CurrentSymbol;
                Reader.ReadNextSymbol();

                if (Reader.CurrentSymbol == 'R' || Reader.CurrentSymbol == 'r')
                {
                    operatorName += Reader.CurrentSymbol;
                    Reader.ReadNextSymbol();

                    // Проверяем, что после R идет точка
                    if (Reader.CurrentSymbol == '.')
                    {
                        Reader.ReadNextSymbol();
                        currentName = null;
                        currentLexem = Lexems.Xor;
                        return;
                    }
                    else
                    {
                        throw new Exception($"Ошибка: Ожидается '.' после .XOR, найден '{Reader.CurrentSymbol}' в строке {Reader.LineNumber}");
                    }
                }
            }
        }
        else
        {
            throw new Exception($"Ошибка: Недопустимый оператор после точки: '{Reader.CurrentSymbol}' в строке {Reader.LineNumber}. Ожидается NOT, AND, OR или XOR");
        }

        throw new Exception($"Ошибка: Недопустимый оператор: {operatorName} в строке {startLine}");
    }

    private static void ParseInteger()
    {
        string integerValue = string.Empty;

        do
        {
            integerValue += Reader.CurrentSymbol;
            Reader.ReadNextSymbol();
        }
        while (char.IsDigit(Reader.CurrentSymbol));

        currentName = integerValue;
        currentLexem = Lexems.IntNumber;
    }

    /// <summary>
    /// Получает идентификатор из исходного кода
    /// </summary>
    private static void ParseIdentifier()
    {
        string identifier = string.Empty;

        do
        {
            identifier += Reader.CurrentSymbol;
            Reader.ReadNextSymbol();
        }
        while (char.IsLetterOrDigit(Reader.CurrentSymbol) && identifier.Length < MaxIdentifierLength);

        if (identifier.Length >= MaxIdentifierLength)
        {
            throw new Exception("Ошибка: Длина идентификатора превышает максимальную допустимую.");
        }

        currentName = identifier;
        currentLexem = GetKeywordLexem(identifier);
    }

    /// <summary>
    /// Текущая лексема, которая анализируется
    /// </summary>
    public static Lexems CurrentLexem => currentLexem;

    /// <summary>
    /// Текущее имя идентификатора
    /// </summary>
    public static string? CurrentName => currentName;
}