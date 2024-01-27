namespace AngularAuthApi.Models
{
    public class EmailModel
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public EmailModel(string to,string subject,string contnet)
        {
            To=to;
            Subject=subject;
            Content=contnet;
        }
    }
}
