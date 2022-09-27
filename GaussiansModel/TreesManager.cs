using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GaussiansModel.Functions;
using System.Collections.ObjectModel;

namespace GaussiansModel
{
    public class TreesManager
    {
        public TreesManager()
        {
            Trees = new();
        }
        public ObservableCollection<FunctionNodeTree> Trees { get; init; }
        public static FunctionNodeTree ReadFile(Stream stream)
        {
            try
            {
                string str = "";
                using (StreamReader sr = new(stream))
                {
                    str = sr.ReadToEnd();
                }
                FunctionNodeTree? res = (FunctionNodeTree?)JsonConvert.DeserializeObject(str, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                if (res == null)
                    throw new Exception();
                return res;
            }
            catch (Exception)
            {

                throw new FileLoadException();
            }

        }

        public static void WriteFile(Stream stream, FunctionNodeTree tree)
        {
            try
            {
                string str = JsonConvert.SerializeObject(tree, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                using (StreamWriter sw = new(stream))
                {
                    sw.Write(str);
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
    }
}
