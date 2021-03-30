using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// .NET Framework 4.6 and 4.5
// To use the ZipFile class, you must reference the System.IO.Compression.FileSystem assembly in your project. 
// ZipArchive needs System.IO.Compression

namespace ZipCheck
{
    class Program
    {
        public static string VersionNumber = "";

        static void Main( string[] args )
        {
            // ---------------------------------------
            // Fr.16.12.2016 15:03:58 -op- "1.0.16.1216" mit "-se:xor2"
            // Fr.15.07.2016 11:35:06 -op- "1.0.16.715" mit Extract und Search
            // Do.14.07.2016 15:08:39 -op- "1.0.16.714", mit 7z*.dll als Content
            // Mi,13.01.2016 11:05:11 -op- "1.0.16.113", Build-Release
            // Fr,18.12.2015 16:56:11 -op- Cr. (C:\Users\otmar.pilgerstorfer\Documents\Visual Studio 2013\Projects\ZipCheck)
            // ----------------------
            // Ref:
            //  D:\Development\...\Dependencies\SevenZipLib.dll
            // als Content dazu, mit Copy if newer: (D:\Development\Intern\Tools\ZipCheck\ZipCheck\SevenZipLib_9.13.2.zip)
            // (D:\Development\...\Dependencies\SevenZipLib\_vonwo.txt)
            //  7z64.dll
            //  7z86.dll
            // Alt:
            // C:\Users\otmar.pilgerstorfer\Documents\Visual Studio 2013\Projects\ZipCheck\ZipCheck\bin\Release\SevenZipLib.dll
            // ---------------------------------------
            VersionNumber = "1.0.16.1216";

            // Check if application was started from within Visual Studio
            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            string moduleName = currentProcess.MainModule.ModuleName;
            bool launchedFromStudio = moduleName.Contains( ".vshost" );

            AddRDToVersion();   // "1.0.16.715.D", "1.0.16.715.R"
            AddDEVToVersion( launchedFromStudio );

            Console_WriteLine( "ZipCheck v:" + VersionNumber );

            bool checkOK = false;

            // Copy string[] args to argParams
            string[] argParams = new string[args.Length];
            Array.Copy( args, argParams, args.Length );

            // Test
#if DEBUG
            // Test - Debug
#else
            // Release
#endif

            // Check Params
            // ZipCheck C:\dir\x.zip y1.dll y2.dll  -s:ver1
            // ZipCheck C:\dir\x.zip y1.dll -s:ver1 y2.dll  -s:ver2
            //          0            1      2       3      4
            string zipFile = "";
            Dictionary<string, int> checkList = new Dictionary<string, int>();
            string searchInFile = "";
            int searchDisplayLength = 50;
            string searchEncoding = "";
            if( argParams.Length >= 1 )
            {
                zipFile = argParams[0].ToString();

                if( argParams.Length >= 2 )
                {
                    for( int i = 1; i < argParams.Length; i++ )
                    {
                        if( argParams[i] != null )
                        {
                            string value = argParams[i].ToString();
                            if( value.Contains( "-s:" ) )
                            {
                                // -s:config.xml
                                var split = value.Split( ':' );
                                searchInFile = split[1];
                            }
                            else if( value.Contains( "-sdl:" ) )
                            {
                                // -sdl:100
                                var split = value.Split( ':' );
                                searchDisplayLength = Convert.ToInt32( split[1] );
                            }
                            else if( value.Contains( "-se:" ) )
                            {
                                // -se:xor2
                                var split = value.Split( ':' );
                                searchEncoding = split[1];
                            }
                            else
                            {
                                try
                                {
                                    // if is already in List
                                    checkList.Add( value, 0 );
                                }
                                catch( Exception )
                                {
                                    //
                                }
                            }
                        }
                    }
                }
            }

            if( !string.IsNullOrEmpty( zipFile ) )
            {
                try
                {
                    checkOK = CheckZip( zipFile, checkList, searchInFile, searchDisplayLength, searchEncoding );
                }
                catch( Exception )
                {
                    //throw;
                }
            }
            else
            {
                // no Parameter
                Console_WriteLine( "Error: no Parameter." );
                Console_WriteLine( @"ZipCheck C:\dir\x.zip y1.dll y2.dll -s:ver1 -sdl:100 -se:xor2" );
            }

            if( launchedFromStudio )
            {
                Console_WriteLine( "" );
                Console_WriteLine( "[DEV] weiter mit einer Taste..." );
                Console.ReadKey();
            }

            // Exit
            if( checkOK )
            {
                System.Environment.Exit( 0 );
            }
            System.Environment.Exit( 1 );
        }

        public static void AddRDToVersion()
        {
#if DEBUG
            VersionNumber += ".D";
#else
            VersionNumber += ".R";
#endif
        }

        public static void AddDEVToVersion( bool launchedFromStudio )
        {
            if( launchedFromStudio )
            {
                VersionNumber += ".[DEV]";
            }
        }

        public static void Console_WriteLine( string txt )
        {
            Debug.WriteLine( txt );
            Console.WriteLine( txt );
        }

        static bool CheckZip( string zipFile, Dictionary<string, int> checkList, string searchInFile, int searchDisplayLength , string searchEncoding )
        {
            bool ret = false;

            string dispSearch = "";
            if( searchInFile != "" )
            {
                dispSearch = ", search for: '" + searchInFile + "'";
            }
            if( searchEncoding != "" )
            {
                dispSearch = ", Encode: '" + searchEncoding + "'";
            }

            Console_WriteLine( "ZipFile: " + zipFile + "" + dispSearch );

            if( !System.IO.File.Exists( zipFile ) )
            {
                Console_WriteLine( "Error: File not found." );

                return ret;
            }

            string zipPath = zipFile;

            //ZipArchive archive = ZipFile.OpenRead( zipPath );

            try
            {
                //SevenZipLib.SevenZipArchive archive = new SevenZipLib.SevenZipArchive( zipPath );

                // ZipArchive
                //using( ZipArchive archive = ZipFile.OpenRead( zipPath ) )
                using( SevenZipLib.SevenZipArchive archive = new SevenZipLib.SevenZipArchive( zipPath ) )
                {
                    //int countAll = archive.Entries.Count;
                    int countAll = archive.Count;
                    string dispCountAll = countAll.ToString();
                    int countLength = dispCountAll.Length;

                    Console_WriteLine( string.Format( "{0} Files in Archiv", countAll ) );

                    string dispCountHeader = "".PadLeft( countLength, '-' );

                    //                                   ## dd.MM.yyyy HH:mm:ss ##.###.### D E FileName
                    string dispHeader = dispCountHeader + " ------------------- ---------- --- ------------------------------------";
                    Console_WriteLine( dispHeader );
                    // Loop
                    int count = 0;
                    //foreach( ZipArchiveEntry file in archive.Entries )
                    foreach( SevenZipLib.ArchiveEntry file in archive )
                    {
                        count++;
                        //string name = file.FullName;
                        string name = file.FileName;
                        //DateTimeOffset date = file.LastWriteTime;
                        DateTime? date = file.LastWriteTime;
                        bool isDir = file.IsDirectory;
                        bool isEncr = file.IsEncrypted;
                        ulong size = file.Size;

                        string dispCount = count.ToString();
                        dispCount = dispCount.PadLeft( countLength, ' ' );

                        bool dispay = false;

                        bool check = false;
                        if( checkList.Count >= 1 )
                        {
                            foreach( var pair in checkList )
                            {
                                string xkey = pair.Key;
                                //if( name.ToLower().Contains( xkey.ToLower() ) )
                                //if( name.IndexOf( xkey, StringComparison.InvariantCultureIgnoreCase ) == 0 )
                                if( name.ToLower() == xkey.ToLower() )
                                {
                                    //pair.Value++;
                                    checkList[xkey] += 1;
                                    check = true;
                                    dispay = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            dispay = true;
                        }

                        if( dispay )
                        {
                            string dispFound = "";
                            if( searchInFile != "" )
                            {
                                // Extract
                                Stream outputStream = new MemoryStream();
                                file.Extract( outputStream );
                                outputStream.Position = 0;
                                int lenght = (int)outputStream.Length;
                                byte[] buffer1 = new byte[lenght];
                                //outputStream.Write( buffer, 0, lenght );
                                BinaryReader reader = new BinaryReader( outputStream );
                                reader.Read( buffer1, 0, lenght );

                                // Encoding
                                if( searchEncoding != "" )
                                {
                                    if( searchEncoding == "xor2" )
                                    {
                                        for( int i = 0; i < lenght; i++ )
                                        {
                                            byte oneByte = buffer1[i];
                                            // xor
                                            // ret = this.Xor( fileText );
                                            //   char chr = str[i];
                                            //   empty = string.Concat( empty, (char)((ushort)(chr ^ '2')) );

                                            oneByte = (byte)(oneByte ^ '2');
                                            buffer1[i] = oneByte;
                                        }
                                    }
                                }

                                // Check Hex(0)
                                bool hasHexNull = false;
                                for( int i = 0; i < lenght; i++ )
                                {
                                    byte oneByte = buffer1[i];
                                    if( oneByte == 0 )
                                    {
                                        hasHexNull = true;
                                    }
                                }

                                // Copy Buffer
                                byte[] buffer2 = new byte[lenght];
                                int cnt2 = 0;
                                for( int i = 0; i < lenght; i++ )
                                {
                                    byte oneByte = buffer1[i];

                                    bool copy = false;
                                    if( oneByte == 10 )
                                    { copy = true; }
                                    if( oneByte == 13 )
                                    { copy = true; }
                                    if( oneByte >= 32 )
                                    { copy = true; }
                                    if( !copy )
                                    {
                                        if( hasHexNull )
                                        {
                                            oneByte = 32;   // 32-Blank, 126-~
                                        }

                                        copy = true;
                                    }
                                    if( copy )
                                    {
                                        buffer2[cnt2] = oneByte;
                                        cnt2++;
                                    }
                                }

                                string result = System.Text.Encoding.UTF8.GetString( buffer2 );

                                // package="at.Comp.App" android:installLocation="auto" android:versionName="0.1.0.22" android:versionCode="22"
                                string search = searchInFile;   // "at.Comp."
                                string searchFound = "";
                                int found = StringSearch( result, search, out searchFound );
                                if( found > 0 )
                                {
                                    int dispLen = searchFound.Length + searchDisplayLength;

                                    if( found + dispLen > result.Length )
                                    {
                                        dispLen = result.Length - found;
                                    }

                                    try
                                    {
                                        dispFound = result.Substring( found, dispLen );
                                    }
                                    catch( Exception ex )
                                    {
                                        //
                                    }
                                    if( hasHexNull )
                                    {
                                        string specialChar = "__";
                                        dispFound = dispFound.Replace( "  ", specialChar );
                                        dispFound = dispFound.Replace( " ", "" );
                                        dispFound = dispFound.Replace( specialChar, " " );
                                    }
                                }
                            }

                            // 07.12.2012 15:24:00 File1.dll
                            // 17.08.2015 11:02:00 File2.dll
                            if( check )
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            string dispIsDir = " ";
                            if( isDir ){ dispIsDir = "D"; }
                            string dispIsEnc = " ";
                            if( isEncr ){ dispIsEnc = "E"; }
                            string dispSize = string.Format( "{0:#,##0}", size );
                            dispSize = "          " + dispSize;
                            dispSize = dispSize.Substring( dispSize.Length - 10 );
                            string disp = string.Format( "{0} {1:dd.MM.yyyy HH:mm:ss} {2}", dispCount, date, name );
                            disp = string.Format( "{0} {1:dd.MM.yyyy HH:mm:ss} {3} {4} {5} {2}", dispCount, date, name, dispSize, dispIsDir, dispIsEnc );
                            Console_WriteLine( disp );
                            Console.ResetColor();

                            if( dispFound != "" )
                            {
                                Console_WriteLine( "-S: " + dispFound + "..." );
                            }

                        }
                    }

                    Console_WriteLine( dispHeader );

                    bool endCheck = true;
                    string filesNotFound = "";
                    if( checkList.Count >= 1 )
                    {
                        foreach( var pair in checkList )
                        {
                            if( pair.Value == 0 )
                            {
                                if( filesNotFound != "" )
                                { filesNotFound += ", "; }

                                filesNotFound += pair.Key;
                                endCheck = false;
                            }
                        }

                        if( endCheck )
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console_WriteLine( "Check OK." );
                            ret = true;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console_WriteLine( "Check not OK: " + filesNotFound );
                        }
                        Console.ResetColor();
                    }
                    else
                    {
                        Console_WriteLine( "OK." );
                        ret = true;
                    }
                }

                //archive.Dispose();
            }
            catch( Exception ex )
            {
                Console_WriteLine( "Error: by open Zip: " + ex.Message );
            }

            //ret = true;
            return ret;
        }

        public static int StringSearch( string txt, string search, out string searchFound )
        {
            int ret = -1;

            ret = txt.IndexOf( search, StringComparison.InvariantCultureIgnoreCase );    // "at.Comp."
            if( ret == -1 )
            {
                search = StringAplitAndJoin( search, " " ); // "a t . j a h a ."
                ret = txt.IndexOf( search, StringComparison.InvariantCultureIgnoreCase );
            }
            searchFound = search;

            return ret;
        }

        public static string StringAplitAndJoin( string txt, string joinString )
        {
            string ret = txt;

            char[] array = txt.ToCharArray();

            ret = string.Join( joinString, array );

            return ret;
        }
    }
}
