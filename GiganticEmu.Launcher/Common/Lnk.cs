using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Shellify;
using Shellify.Core;

namespace GiganticEmu.Launcher;

public static class Lnk
{
    private class PIDL : IEnumerable<PIDL>
    {
        public IntPtr Ptr { get; }

        public PIDL(IntPtr pidl)
        {
            Ptr = pidl;            
        }

        public PIDL(PIDL pidl)
        {
            Ptr = pidl.Ptr;
        }

        public bool IsEmpty { 
            get
            {
                if (Ptr == IntPtr.Zero)
                    return true;

                byte[] bytes = new byte[2];
                Marshal.Copy(Ptr, bytes, 0, 2);
                int size = bytes[0] + bytes[1] * 256;
                return size <= 2;
            }
        }

        public int Size
        {
            get
            {
                if (Ptr.Equals(IntPtr.Zero))
                {
                    return 0;
                }
                
                byte[] buffer = new byte[2];
                Marshal.Copy(Ptr, buffer, 0, 2);
                return buffer[1] * 256 + buffer[0];
            }
        }

        public IEnumerable<byte> Data()
        {
            var size = Marshal.ReadByte(Ptr, 0) + Marshal.ReadByte(Ptr, 1) * 256 - 2;
            
            for (var i = 0; i < size; i++)
            {
                yield return Marshal.ReadByte(Ptr, i + 2);
            }
        }

        System.Collections.IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<PIDL> GetEnumerator()
        {
            var cur = this;
            while (!cur.IsEmpty)
            {
                yield return cur;
                cur = new PIDL(new IntPtr((long)cur.Ptr + cur.Size));
            }
        }
    }
    
    private static class NativeMethods
    {
        [DllImport("shell32.dll", ExactSpelling=true)]
        public static extern void ILFree(IntPtr pidlList);

        [DllImport("shell32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern IntPtr ILCreateFromPathW(string pszPath);
    }

    public static void Create(string filename, string target)
    {
        var link = new ShellLinkFile();
            
        var targetInfo = new FileInfo(target);
            
        link.Header = new ShellLinkHeader
        {
            FileAttributes = targetInfo.Attributes,
            AccessTime = targetInfo.LastAccessTime,
            CreationTime = targetInfo.CreationTime,
            WriteTime = targetInfo.LastWriteTime,
            FileSize = Convert.ToInt32(targetInfo.Length),
            ShowCommand = ShowCommand.Normal,
            LinkFlags = LinkFlags.HasRelativePath | LinkFlags.HasWorkingDir | LinkFlags.ForceNoLinkInfo | LinkFlags.ForceNoLinkTrack | LinkFlags.HasLinkTargetIDList
        };

        link.WorkingDirectory = targetInfo.DirectoryName;
        link.RelativePath = targetInfo.Name;
            
        var pidlList = NativeMethods.ILCreateFromPathW(targetInfo.FullName);
        if (pidlList != IntPtr.Zero)
        {
            try
            {
                var pidl = new PIDL(pidlList);
                link.ShItemIDs = pidl.Select(id => new ShItemID()
                {
                    Data = id.Data().ToArray(),
                }).ToList();
            }
            finally
            {
                NativeMethods.ILFree(pidlList);
            }
        }
        
        link.SaveAs(filename);
    }
}