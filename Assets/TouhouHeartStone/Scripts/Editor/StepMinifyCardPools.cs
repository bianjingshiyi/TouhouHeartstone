using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomBuilder.Editor
{
    [BuilderStep(1)]
    public class StepMinifyCardPools : IBuilderStep
    {
        public void ActionBefore(object options)
        {
            Queue<string> dirsToCheck = new Queue<string>();
            dirsToCheck.Enqueue(Application.streamingAssetsPath);

            while (dirsToCheck.Count > 0)
            {
                var currentDir = dirsToCheck.Dequeue();

                var files = Directory.GetFiles(currentDir);
                foreach (var item in files)
                {
                    if (Path.GetExtension(item) == ".ddcp")
                    {
                        MinifyCardPool(item);
                    }
                }

                var dirs = Directory.GetDirectories(currentDir);
                foreach (var item in dirs)
                {
                    dirsToCheck.Enqueue(item);
                }
            }
        }
        public void ActionAfter()
        {

        }
        private void MinifyCardPool(string path)
        {
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            if (lines == null || lines.Count < 2)
            {
                throw new FileLoadException("Card pool file " + path + " is empty.");
            }

            string header = lines[0];
            StringBuilder sb = new StringBuilder();
            foreach (var line in lines.Skip(1))
            {
                var newLine = line.Trim('\n', '\r', ' ');
                sb.Append(newLine);
            }
            string cardPoolStr = sb.ToString();

            using (StreamWriter sw = new StreamWriter(path, false))
            {
                sw.WriteLine(header);
                sw.WriteLine(cardPoolStr);
            }

            Debug.Log($"Card pool file {path} minified.");
        }
    }
}
