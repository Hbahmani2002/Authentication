using System.ComponentModel.DataAnnotations;

namespace AngularAuthApi.Models.Api
{
    public class Files
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int Size { get; set; }
        public DateTime Createdat { get; set; }
        public DateTime modifiedat { get; set; }
    }
}
