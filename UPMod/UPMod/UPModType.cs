using Patchwork.Attributes;
using System;
using System.IO;

namespace UPMod
{
    [PatchInfo]
    public class UPModType : IPatchInfo
    {
        public string PatchVersion
        {
            get
            {
                return "1.1.0";
            }
        }

        public string Requirements
        {
            get
            {
                return "None";
            }
        }

        public string PatchName
        {
            get
            {
                return "UPMod";
            }
        }

        public FileInfo GetTargetFile(AppInfo app)
        {
            if (app == null)
                return null;

            DirectoryInfo baseDir = app.BaseDirectory;
            string fullName = "";

            if (baseDir != null)
                fullName = baseDir.FullName;

            string[] pathArray = new string[]
            {
                fullName,
                "PillarsOfEternity_Data",
                "Managed",
                "Assembly-CSharp.dll"
            };

            string fullPath = String.Join("/", pathArray);
            FileInfo fileInfo = new FileInfo(fullPath);
            return fileInfo;

            // original: (taken from IEMod; but doesn't work)
            //string fileName = PathHelper.Combine(new string[]
            //{
            //    fullName,
            //    "PillarsOfEternity_Data",
            //    "Managed",
            //    "Assembly-CSharp.dll"
            //});
            //return new FileInfo(fileName);
        }

        public string CanPatch(AppInfo app)
        {
            return null;
        }
    }
}
