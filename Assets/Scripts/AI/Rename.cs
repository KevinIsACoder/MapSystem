using UnityEngine;

public class Rename : MonoBehaviour
{
    public string fromDir;
    public string toDir;
    public string oldExtension = ".story";
    public string newExtension = ".txt";

    [ContextMenu("Rename Files")]
    public void RenameFiles()
    {
        var dirInfo = new System.IO.DirectoryInfo(fromDir);

        RenameInternal(dirInfo);

        void RenameInternal(System.IO.DirectoryInfo dirInfo)
        {
            foreach (var fsi in dirInfo.GetFileSystemInfos())
            {
                if (fsi is System.IO.FileInfo)
                {
                    if (fsi.Extension == oldExtension)
                    {
                        //copy file 
                        var fileName = fsi.Name;
                        var newFileName = fileName.Replace(oldExtension, newExtension);
                        System.IO.File.Copy(fsi.FullName, System.IO.Path.Combine(toDir, newFileName));
                    }
                }
                else
                {
                    var directory = fsi as System.IO.DirectoryInfo;
                    if (directory != null)
                    {
                        RenameInternal(directory);
                    }
                }
            }
        }
    }
}