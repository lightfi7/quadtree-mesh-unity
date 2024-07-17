using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct DSM
{
    public double latitude;
    public double longitude;
    public double height;
};

public class DSMLoader : MonoBehaviour
{
    public static DSMLoader instance;

    Thread thread;
    public string directoryPath = "";

    [HideInInspector]
    public List<DSM> dSMs;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        dSMs= new List<DSM>();
        thread = new Thread(LoadDSM, 10);
        thread.Start();
        // thread.Join();
    }

    void LoadDSM()
    {
        string[] filePaths = Directory.GetFiles(directoryPath, "*.bin", SearchOption.AllDirectories);

        for (int i = 0; i < filePaths.Length; i++)
        {
            using (var fileStream = File.Open(filePaths[i], FileMode.Open, FileAccess.Read))
            {
                var length = fileStream.Length;
                using (var memoryMappedFile = MemoryMappedFile.CreateFromFile(fileStream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, true))
                {
                    using (var memoryMappedView = memoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
                    {
                        var position = 0L;
                        while (position < length)
                        {
                            var longitude = memoryMappedView.ReadDouble(position);
                            position += sizeof(double);
                            var latitude = memoryMappedView.ReadDouble(position);
                            position += sizeof(double);
                            var height = memoryMappedView.ReadDouble(position);
                            position += sizeof(double);
                            
                            dSMs.Add(new DSM
                            {
                                latitude = latitude,
                                longitude = longitude,
                                height = height
                            });
                        }
                    }
                }
            }
        }
    }

  
}
