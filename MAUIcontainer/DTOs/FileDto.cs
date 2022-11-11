using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUIcontainer {
    public class FilesRequest { 
        public List<FileDto> Files { get; set; }
        public RequestDto Request { get; set; }
    }
    public class FileDto {
        public string Name { get; set; }
        public string Description { get; set; }
        public  string FilePath { get; set; }
        public string ContentType { get; set; }
        public string Src { get; set; }
    }
}
