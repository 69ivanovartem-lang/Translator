using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Translator.Core;
using Translator.View.ViewModel.Core;

namespace Translator.View.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private RelayCommand? _openFileCommand;
        private RelayCommand? _saveFileCommand;
        private RelayCommand? _compileCommand;
        private RelayCommand? _compileAndRunCommand;
        private RelayCommand? _saveAsmCommand;

        public RelayCommand OpenFileCommand => _openFileCommand ??= new RelayCommand(obj => OpenFile());
        public RelayCommand SaveFileCommand => _saveFileCommand ??= new RelayCommand(obj => SaveFile());
        public RelayCommand CompileCommand => _compileCommand ??= new RelayCommand(obj => Compile());
        public RelayCommand CompileAndRunCommand => _compileAndRunCommand ??= new RelayCommand(obj => CompileAndRun());
        public RelayCommand SaveAsmCommand => _saveAsmCommand ??= new RelayCommand(obj => SaveAsm());

        private void OpenFile()
        {
            var dialog = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (dialog.ShowDialog() == true)
            {
                InputText = File.ReadAllText(dialog.FileName);
            }
        }

        private void SaveFile()
        {
            var dialog = new SaveFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, InputText);
            }
        }

        private void SaveAsm()
        {
            if (string.IsNullOrWhiteSpace(OutputText))
            {
                MessageBox.Show("Сначала скомпилируйте программу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dialog = new SaveFileDialog { Filter = "ASM files (*.asm)|*.asm|All files (*.*)|*.*", FileName = "output.asm" };
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, OutputText);
                MessageBox.Show($"ASM код сохранен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Compile()
        {
            try
            {
                var syntaxAnalyzer = new SyntaxAnalyzer();
                syntaxAnalyzer.Compile(InputText);
                OutputText = string.Join("\n", CodeGenerator.GetGeneratedCode());
                MessageBox.Show("Компиляция успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                OutputText = ex.Message;
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompileAndRun()
        {
            try
            {
                // Сначала компилируем
                var syntaxAnalyzer = new SyntaxAnalyzer();
                syntaxAnalyzer.Compile(InputText);
                var code = string.Join("\n", CodeGenerator.GetGeneratedCode());
                OutputText = code;

                // Путь к DOSBox
                string dosBoxPath = @"D:\Translator\Translator\Translator.View\bin\Debug\net8.0-windows\DOSBox\DOSBox.exe";
                string workDir = @"C:\TranslatorTemp";

                if (!Directory.Exists(workDir))
                    Directory.CreateDirectory(workDir);

                // Сохраняем ASM файл
                string asmPath = Path.Combine(workDir, "compile.asm");
                File.WriteAllText(asmPath, code);

                // Копируем MASM и LINK
                string dosBoxDir = Path.GetDirectoryName(dosBoxPath);
                File.Copy(Path.Combine(dosBoxDir, "MASM.EXE"), Path.Combine(workDir, "MASM.EXE"), true);
                File.Copy(Path.Combine(dosBoxDir, "LINK.EXE"), Path.Combine(workDir, "LINK.EXE"), true);

                // Создаем BAT файл
                string batContent = @"@echo off
MASM.EXE compile.asm;
LINK.EXE compile.obj;
compile.exe
pause";
                File.WriteAllText(Path.Combine(workDir, "build.bat"), batContent);

                // Создаем конфиг DOSBox
                string confContent = $@"[autoexec]
mount C ""{workDir}""
C:
build.bat
exit";
                File.WriteAllText(Path.Combine(workDir, "dosbox.conf"), confContent);

                // Запускаем DOSBox
                Process.Start(new ProcessStartInfo
                {
                    FileName = dosBoxPath,
                    Arguments = $"-conf \"{Path.Combine(workDir, "dosbox.conf")}\"",
                    UseShellExecute = false
                });

                MessageBox.Show("DOSBox запущен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string _inputText = @"Boolean x, y, z
Begin
    x := True
    y := False
    z := x .AND. y
    z := x .OR. y
    z := .NOT. z
End
Print z";

        private string _outputText = "";

        public string InputText { get => _inputText; set { _inputText = value; OnPropertyChanged(); } }
        public string OutputText { get => _outputText; set { _outputText = value; OnPropertyChanged(); } }
    }
}