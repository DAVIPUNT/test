using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using System.IO;
using Ionic.Zip;

namespace SftpManager
{
    /// <summary>
    /// Utility package that exposes static methods to interact with a SFTP server.
    /// </summary>
    public static class SftpManager
    {
        /// <summary>
        /// Method that lists the content of a directory on a SFTP server
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="path"></param>
        /// <returns>A tuple containing the file Name as first item and the LastWriteTimeUTC as the second item</returns>
        public static List<Tuple<String, DateTime>> ListDirectory(string host, int port, string user, string pass, string path)
        {
            SftpClient cli = null;
            // returns empty string if no result
            List<Tuple<String, DateTime>> retVal = new List<Tuple<String, DateTime>>();
            // idea: select only newer files
            try
            {

                cli = new SftpClient(host, port, user, pass);

                cli.Connect();

                foreach (Renci.SshNet.Sftp.SftpFile file in cli.ListDirectory(path))
                {
                    if (!".".Equals(file.Name))
                    {
                        retVal.Add(Tuple.Create(file.Name, file.LastWriteTimeUtc));
                    }
                }
            }
            catch(Exception ex)
            {
                // TODO
                Console.Error.WriteLine(ex.Message);
            }
            finally
            {
                if(cli != null)
                {
                    cli.Disconnect();
                }
            }

            return retVal;

        }

        /// <summary>
        /// Method that downloads and extracts a single file from a specific SFTP server directory to a specified target path.
        /// It does not preserve the file timestamp
        /// </summary>
        /// <param name="host">SFTP server host</param>
        /// <param name="port">SFTP server port</param>
        /// <param name="user">SFTP server username</param>
        /// <param name="pass">SFTP server password</param>
        /// <param name="filename">Name of the file to download</param>
        /// <param name="inPath">Folder on the SFTP Server where the file to be downloaded is located</param>
        /// <param name="outPath">Target folder for the extracted file</param>
        /// <returns>The absolute path of the extracted file</returns>
        public static string GetAndUnzip(string host, int port, string user, string pass, string filename, string inPath, string outPath)
        {
            return GetAndUnzip(host, port, user, pass, filename, inPath, outPath, false);
        }

        /// <summary>
        /// Method that downloads and extracts a single file from a specific SFTP server directory to a specified target path.
        /// </summary>
        /// <param name="host">SFTP server host</param>
        /// <param name="port">SFTP server port</param>
        /// <param name="user">SFTP server username</param>
        /// <param name="pass">SFTP server password</param>
        /// <param name="filename">Name of the file to download</param>
        /// <param name="inPath">Folder on the SFTP Server where the file to be downloaded is located</param>
        /// <param name="outPath">Target folder for the extracted file</param>
        /// <param name="getZipTimestamp">Flag that states if the extracted file has to have the same timestamp of the zip file</param>
        /// <returns>The absolute path of the extracted file</returns>
        public static string GetAndUnzip(string host, int port, string user, string pass, string filename, string inPath, string outPath, bool getZipTimestamp)
        {

            // returns empty string if no result
            string retVal = "";

            string p = Path.GetTempPath();
            // temporary file path
            string tmp = Path.Combine(p,filename);
            try
            {

                string f = Get(host, port, user, pass, filename, inPath, p);

                using (ZipFile zip = ZipFile.Read(f))
                {
                    // supposing that there is only one file per archive
                    foreach (ZipEntry zipEntry in zip)
                    {
                        retVal = Path.Combine(outPath, zipEntry.FileName);
                        using (FileStream fs = new FileStream(retVal, FileMode.Create))
                        {
                            zipEntry.Extract(fs);
                        }

                        // preserving the ZIP LastWriteTimeUtc to check if already downloader
                        if (getZipTimestamp)
                        {
                            System.IO.File.SetLastWriteTimeUtc(retVal, System.IO.File.GetLastWriteTimeUtc(f));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                File.Delete(tmp);
            }

            return retVal;
        }

        /// <summary>
        /// Method that downloads a single file from a specific SFTP server directory to a specified target path.
        /// </summary>
        /// <param name="host">SFTP server host</param>
        /// <param name="port">SFTP server port</param>
        /// <param name="user">SFTP server username</param>
        /// <param name="pass">SFTP server password</param>
        /// <param name="filename">Name of the file to download</param>
        /// <param name="inPath">Folder on the SFTP Server where the file to be downloaded is located</param>
        /// <param name="outPath">Target folder for the downloaded file</param>
        /// <returns>The absolute path of the downloaded file</returns>
        public static string Get(string host, int port, string user, string pass, string filename, string inPath, string outPath)
        {
            SftpClient cli = null;

            // returns empty string if no result
            string retVal = "";

            var f = Path.Combine(outPath, filename);
            try
            {

                cli = new SftpClient(host, port, user, pass);

                cli.Connect();

                using (FileStream fs = File.OpenWrite(f))
                {
                    cli.DownloadFile(inPath + filename, fs);
                }
                retVal = f;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                if (cli != null)
                {
                    cli.Disconnect();
                }
            }

            return retVal;
        }

    }

}
