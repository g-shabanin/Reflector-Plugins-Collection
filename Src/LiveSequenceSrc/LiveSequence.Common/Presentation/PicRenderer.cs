using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using LiveSequence.Common.Domain;

namespace LiveSequence.Common.Presentation
{
    public class PicRenderer : IRenderer
    {
        List<ObjectInstanceInfo> objectList;

        public string Export(SequenceData data)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            String filePath = Path.Combine(currentDirectory, "tempseq.pic");

            using (var sw = new StreamWriter(filePath, false))
            {
                Write(sw, ".PS");
                Write(sw, "copy \"sequence.pic\";");
                Write(sw, "maxpswid = 20;");
                Write(sw, "maxpsht = 20;");

                Write(sw, "boxwid = 1.9;");
                Write(sw, "movewid = 0.1;");
                Write(sw, "spacing = 0.3;");

                Write(sw, "step();");
                objectList = data.GetObjectList();

                foreach (ObjectInstanceInfo oInfo in objectList)
                {
                    WriteObject(sw, oInfo.TypeName, oInfo.Key);
                    //Write(sw, "active(" + oInfo.Key + ");");
                }

                Write(sw, "step();");
                Write(sw, "step();");

                foreach (MethodCallInfo mInfo in data.GetMethodList())
                {
                    WriteMessage(sw, mInfo);
                }

                Write(sw, "step();");
                Write(sw, "step();");

                // reverse iterate the object collection to put complete
                objectList.Reverse();

                foreach (ObjectInstanceInfo oInfo in objectList)
                {
                    WriteComplete(sw, oInfo.Key);
                }

                Write(sw, ".PE");
            }

            return this.ExecutePlot(currentDirectory, filePath, data.ToString());
        }

        private string ExecutePlot(string currentDirectory, string filePath, string targetFileName)
        {
            string extension = Settings.OutputType;

            string targetFilePath = Path.Combine(currentDirectory, string.Format("{0}.{1}", targetFileName, extension));

            string plotFileName = Settings.Pic2PlotPath();
            string fileName = Path.Combine(currentDirectory, "RunPlot.bat");
            string arguments = string.Format(" \"{0}\" {1} \"{2}\" \"{3}\"", plotFileName, extension, filePath, targetFilePath);
            
            ProcessStartInfo pStartInfo = new ProcessStartInfo
                                              {
                                                  WorkingDirectory = currentDirectory,
                                                  WindowStyle = ProcessWindowStyle.Hidden,
                                                  CreateNoWindow = true,
                                                  FileName = fileName,
                                                  Arguments = arguments
                                              };

            Logger.Current.Debug("Process Name:" + fileName);
            Logger.Current.Debug("Arguments:" + arguments);

            Process.Start(pStartInfo);

            return targetFilePath;
        }

        private void WriteComplete(StreamWriter sw, string key)
        {
            Write(sw, string.Format("complete({0});", key));
        }

        private void WriteMessage(StreamWriter sw, MethodCallInfo mInfo)
        {
            // get the key
            string sourceKey = objectList.Find(
                oInfo => oInfo.TypeName.Equals(mInfo.TypeName)
                ).Key;

            string targetKey = objectList.Find(
                oInfo => oInfo.TypeName.Equals(mInfo.MethodCallType)
                ).Key;

            Write(sw, string.Format("message({0},{1}, \"{2}\");",sourceKey, targetKey, mInfo.MethodCallName));
        }

        public bool FindObjectInstance(string objectType, string methodType)
        {
            return objectType.Equals(methodType);
        }

        public void WriteObject(StreamWriter sw, string type, string key)
        {
            Write(sw, string.Format("object({0},\"{1}:{2}\");", key, key.ToLower(), type));
        }

        public void Write(StreamWriter sw, string message)
        {
            sw.Write(message + "\n");
        }
    }
}