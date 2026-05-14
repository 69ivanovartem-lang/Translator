/// <summary>
/// Статический класс, отвечающий за чтение символов из файла.
/// </summary>
public static class Reader
{
    private static int lineNumber;
    private static int characterPositionInLine;
    private static int currentSymbol;
    private static string code = string.Empty;

    /// <summary>
    /// Номер текущей строки.
    /// </summary>
    public static int LineNumber => lineNumber;

    /// <summary>
    /// Позиция текущего символа в строке.
    /// </summary>
    public static int CharacterPositionInLine => characterPositionInLine;

    /// <summary>
    /// Текущий читаемый символ.
    /// </summary>
    public static char CurrentSymbol
    {
        get
        {
            if (currentSymbol >= 0 && currentSymbol < code.Length)
                return code[currentSymbol];
            return EndOfFile;
        }
    }

    /// <summary>
    /// Константа, представляющая конец файла.
    /// </summary>
    public const char EndOfFile = '\0';

    /// <summary>
    /// Читает следующий символ из файла и обновляет состояние строки и позиции.
    /// </summary>
    public static void ReadNextSymbol()
    {
        currentSymbol++;

        if (currentSymbol >= code.Length)
        {
            currentSymbol = code.Length; // Устанавливаем на позицию после последнего символа
            return;
        }

        if (code[currentSymbol] == '\n')
        {
            lineNumber++;
            characterPositionInLine = 0;
        }
        else if (code[currentSymbol] == '\r')
        {
            // Пропускаем CR, обрабатываем только вместе с LF
            if (currentSymbol + 1 < code.Length && code[currentSymbol + 1] == '\n')
            {
                // Пропускаем CR, следующий ReadNextSymbol обработает LF
            }
            else
            {
                characterPositionInLine++;
            }
        }
        else if (code[currentSymbol] == '\t')
        {
            characterPositionInLine += 4; // Табуляция как 4 пробела
        }
        else
        {
            characterPositionInLine++;
        }
    }

    /// <summary>
    /// Инициализирует чтение из указанного файла.
    /// </summary>
    /// <param name="code">Исходный файл для чтения.</param>
    public static void Initialize(string code)
    {
        Reader.code = code;
        currentSymbol = -1;
        lineNumber = 1;
        characterPositionInLine = 0;
        ReadNextSymbol();
    }
}