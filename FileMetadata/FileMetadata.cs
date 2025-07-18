using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileMetadata
{
    public class FileMetadata
    {
        public string path;
        public FileInfo info;
        public FileAttributes attrs;
        public FileVersionInfo verinfo;

        public bool hidden;
        public bool _readonly; //add _ because readonly is a native modifier and so can't be used
        public bool isSymlink;
        //Explicitly define them for simplicity

        public FileMetadata(string _path)
        {
            path = _path;
            info = new FileInfo(path);
            attrs = File.GetAttributes(path);
            verinfo = FileVersionInfo.GetVersionInfo(path);

            hidden = (attrs & FileAttributes.Hidden) == FileAttributes.Hidden;
            _readonly = (attrs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            isSymlink = (attrs & FileAttributes.ReparsePoint) != 0;
            //Bitwise OR ( | ), which means multiple flags is possible, ensure for both cases
        }

        public string GetPath()
        {
            return path;
        }

        //File info
        public FileInfo GetFileInfo()
        {
            return new FileInfo(path);
        }

        public bool SetInfo(DateTime? creation = null, DateTime? lastAccess = null, DateTime? lastWrite = null)
        {
            if (info.Exists)
            {
                try
                {
                    if (creation.HasValue)
                        info.CreationTime = creation.Value;
                    if (lastAccess.HasValue)
                        info.LastAccessTime = lastAccess.Value;
                    if (lastWrite.HasValue)
                        info.LastWriteTime = lastWrite.Value;

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public FileVersionInfo GetVersionInfo()
        {
            return verinfo;
        }

        public string GetFileVersion()
        {
            return verinfo.FileVersion;
        }

        public string GetProductVersion()
        {
            return verinfo.ProductVersion;
        }
        //FileVersion and ProductVersion are already included in FileVersionInfo, these are just shortcuts


        //File hash
        public string GetMD5(string filePath) => GetHash(filePath, MD5.Create());

        public string GetSHA1(string filePath) => GetHash(filePath, SHA1.Create());

        public string GetSHA256(string filePath) => GetHash(filePath, SHA256.Create());
        
        public string GetSHA512(string filePath) => GetHash(filePath, SHA512.Create());



        public string GetHash(string filePath, HashAlgorithm algorithm)
        {
            try
            {
                using (algorithm)
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = algorithm.ComputeHash(stream);
                    var sb = new StringBuilder();
                    foreach (var b in hashBytes)
                        sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }
                //Closing of the file is automatically called because of the "using" block + it being a stream
                //Same goes for the HashAlgorithm
            }
            catch
            {
                return null;
            }
        }

        //File size
        public long GetSizeInBytes()
        {
            return info.Length;
        }
        public string GetSizeFormatted(int decimals = 2)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = info.Length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len.ToString($"F{decimals}")} {sizes[order]}";
        }

        //File attributes (hidden, readonly, locked aka in use by another process)
        public bool IsHidden()
        {
            return hidden;
        }

        public bool IsReadOnly()
        {
            return _readonly;
        }

        public bool IsLocked()
        {
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return false; //can open, not locked
                }
            }
            catch (IOException)
            {
                return true; //can't open, locked
            }
        }

        public bool IsSymlink()
        {
            return isSymlink;
        }
    }
}
