/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

using NFX.Web;
using NFX.Environment;
using NFX.IO.FileSystem;

namespace NFX.Wave.Handlers
{
  /// <summary>
  /// Downloads local files or files from portal content file system.
  /// Be carefull with this handler as the incorrect root setup may allow users to download system-internal files
  /// </summary>
  public class FileDownloadHandler : WorkHandler
  {

     public const string VAR_FILE_PATH  = "filePath";
     public const string VAR_ATTACHMENT = "attachment";
     public const string VAR_CHUNKED    = "chunked";
     
     public const string INVALID_ROOT   = @"Invalid-Root-Path:\";

     protected FileDownloadHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher, name, order, match)
     {
     }


     protected FileDownloadHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
     {
        ConfigAttribute.Apply(this, confNode);
     }
    
     [Config]
     private string m_RootPath;
     
     [Config(Default=true)]
     private bool m_Throw = true;
     
     [Config]
     private int m_CacheMaxAgeSec;

     [Config]
     private bool m_UsePortalHub;

     /// <summary>
     /// Specifies local root path
     /// </summary>
     public string RootPath 
     {
        get {return m_RootPath.IsNullOrWhiteSpace() ? INVALID_ROOT : m_RootPath;}
        set {m_RootPath = value;}
     }

     /// <summary>
     /// Specifies whether the handler generates simple 404 text or throws
     /// </summary>
     public bool Throw 
     {
        get {return m_Throw;}
        set {m_Throw = value;}
     }

     /// <summary>
     /// Specifies the maximum age in cache in seconds. Zero means - do not cache the file
     /// </summary>
     public int CacheMaxAgeSec 
     {
        get {return m_CacheMaxAgeSec;}
        set {m_CacheMaxAgeSec = value<0 ? 0 : value;}
     }

     /// <summary>
     /// When true, downloads files from PortalHub.ContentFileSystem for selected portal
     /// </summary>
     public bool UsePortalHub
     {
       get { return m_UsePortalHub;}
       set { m_UsePortalHub = value;}
     }

    
     protected override void DoHandleWork(WorkContext work)
     {
         var fp         = work.MatchedVars[VAR_FILE_PATH].AsString("none");
         var attachment = work.MatchedVars[VAR_ATTACHMENT].AsBool(true); 
         var chunked    = work.MatchedVars[VAR_CHUNKED].AsBool(true); 

         //Sanitize
         fp = fp.Replace("..", string.Empty)
                .Replace(":/", string.Empty)
                .Replace(@"\\", @"\");


         string fileName = null;
         IFileSystem fs = null;
         FileSystemSession fsSession = null;
         FileSystemFile fsFile = null;
         bool exists = false;

         if (m_UsePortalHub)
         {
            var hub = PortalHub.Instance;
            fs = hub.ContentFileSystem;
            fileName = m_RootPath!=null ? fs.CombinePaths(hub.ContentFileSystemRootPath, m_RootPath, fp)
                                        : fs.CombinePaths(hub.ContentFileSystemRootPath, fp);
            fsSession = fs.StartSession(hub.ContentFileSystemConnectParams);
            fsFile = fsSession[fileName] as FileSystemFile;
            exists = fsFile!=null;
         }
         else
         {
            fileName = Path.Combine(RootPath, fp);
            exists = File.Exists(fileName);
         }

         try
         {
             if (!exists)
             {
               var text = StringConsts.FILE_DL_HANDLER_NOT_FOUND_INFO.Args(fileName);
               if (m_Throw)
                throw new HTTPStatusException(SysConsts.STATUS_404, SysConsts.STATUS_404_DESCRIPTION, text);  
           
               work.Response.ContentType = ContentType.TEXT;
               work.Response.Write( text );
               work.Response.StatusCode = SysConsts.STATUS_404;
               work.Response.StatusDescription = SysConsts.STATUS_404_DESCRIPTION;
               return;
             }

             if (!work.Response.WasWrittenTo)
               work.Response.Buffered = !chunked;
    
             if (m_CacheMaxAgeSec>0)
              work.Response.Headers[HttpResponseHeader.CacheControl] = "private, max-age={0}, must-revalidate".Args(m_CacheMaxAgeSec);
             else
              work.Response.Headers[HttpResponseHeader.CacheControl] = "no-cache";

             if (fsFile==null)
               work.Response.WriteFile(fileName, attachment: attachment);
             else
             {
               var ext = Path.GetExtension(fsFile.Name);
               work.Response.ContentType = NFX.Web.ContentType.ExtensionToContentType(ext, NFX.Web.ContentType.BINARY);
               work.Response.WriteStream(fsFile.FileStream, attachmentName: attachment ? Path.GetFileName(fileName) : null);
             }
         }
         finally
         {
           DisposableObject.DisposeAndNull(ref fsSession);
         }
     }
  }
}
