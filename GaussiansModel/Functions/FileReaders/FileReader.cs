using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GaussiansModel.Extension;
using Newtonsoft.Json.Serialization;

namespace GaussiansModel.Functions
{
    /// <summary>
    /// Менеджер классов по чтению графика из файла
    /// </summary>


    #region BaseFileReader

    /// <summary>
    /// Аттрибут, который необходимо давать всем классам - реализаторам IFileReader
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FileReaderAttribute : Attribute
    {
    }

    public interface IFileReader
    {
        public PointGraph ReadFile(Stream file);
    }

    /// <summary>
    /// Реализация базового класса для чтения - одиночки
    /// </summary>
    /// <typeparam name="T">Тип наследуемого класса</typeparam>
    public abstract class FileReaderBase<T> : NodeFunctionBase<T>, IFileReader where T : FileReaderBase<T>
    {
        public FileReaderBase()
        {
            Inputs.Add(new FunctionParameter(Properties.Resources.InputSource, typeof(IEnumerable<StreamData>), new List<StreamData>()));
            Outputs.Add(new FunctionParameter(Properties.Resources.OutputGraphs, typeof(IEnumerable<PointGraph>), new List<PointGraph>()));
        }

        public override void Invoke(CancellationToken token)
        {
            var fileNames = ((IEnumerable<StreamData>)FindInputParameter(Properties.Resources.InputSource).Value).Select(i => i.FileName);
            List<PointGraph> res = new();
            foreach (var fileName in fileNames)
            {
                using (FileStream stream = new(fileName, FileMode.Open))
                {
                    res.Add(ReadFile(stream));
                }
            }
            SetOutputParameter(Properties.Resources.OutputGraphs, res);
        }

        public abstract PointGraph ReadFile(Stream file);
    }
    #endregion//BaseFileReader

    #region RealizationsFileReader
    /// <summary>
    /// Некоторые скрипты
    /// </summary>
    internal static class ReaderSkripts
    {
        /// <summary>
        /// Добавляет лишь те строки, которые удовлетворяют условию
        /// </summary>
        /// <param name="file">Считывемый файл</param>
        /// <param name="canAddLineFunc">Функция условия добавления стоки</param>
        /// <returns></returns>
        public static IEnumerable<string> BuildStringArrayByFunction(Stream file, Func<string, bool> canAddLineFunc)
        {
            List<string> res = new();
            using (StreamReader sr = new(file))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                    if (canAddLineFunc(line))
                        res.Add(line);
            }
            return res;
        }
    }

    [FileReader]
    public class ReaderFileByPattern : FileReaderBase<ReaderFileByPattern>
    {
        public ReaderFileByPattern() { }
        Regex regex = new(@"^\s*-?\d+([.|,]\d*)?([e|E]-\d*)?\s*-?\d+([.|,]\d*)?([e|E]-\d*)?\s*$");
        public override PointGraph ReadFile(Stream file)
        {
            var strings = ReaderSkripts.BuildStringArrayByFunction(file, i => regex.Match(i).Success);
            return new(strings.Select(i =>
            {
                var strs = i.Split(new char[] { ' ', '\t', '\r' }).Where(i => i.Count() > 0).Select(i => i.Replace(',', '.'));
                if (strs.Count() != 2)
                    throw new FileLoadException();
                return new Point(ConvertExtension.ToDoubleRich(strs.ElementAt(0)),
                    ConvertExtension.ToDoubleRich(strs.ElementAt(1)));
            }));
        }
        public override string GetName() => Properties.Resources.ByPatternReaderName;
    }

    public class StreamData
    {
        public StreamData(string fileName, FileMode mode)
        {
            FileName = fileName;
            Mode = mode;
        }
        public string FileName { get; set; }
        public FileMode Mode { get; set; }
    }
    #endregion//RealizationsFileReader
}
