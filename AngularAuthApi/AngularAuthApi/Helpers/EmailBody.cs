namespace AngularAuthApi.Helpers
{
    public static class EmailBody
    {
       public static string EmailStringBody(string email,string emailToken) {
            return







                    $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset</title>
</head>
<body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; color: #333;"">

    <div style=""max-width: 600px; margin: 0 auto; padding: 20px; background-color: #fff; border-radius: 5px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
        <h2 style=""color: #007bff;"">Password Reset</h2>
        <p>Hello [Username],</p>
        <p>We received a request to reset your password. If you did not make this request, please ignore this email.</p>
        <p>To reset your password, click the link below:</p>
        <p><a href=""http://localhost:4200/reset?email={email}&code={emailToken}""  style=""display: inline-block; padding: 10px 20px; background-color: #007bff; color: #fff; text-decoration: none; border-radius: 5px;"">Reset Password</a></p>
        <p>If the above link doesn't work, copy and paste the following URL into your browser:</p>
        <p>[RESET_LINK]</p>
        <p>This link will expire in [EXPIRY_TIME] minutes.</p>
        <p>Thank you,<br>Your App Team</p>
    </div>

</body>
</html>";        
                
         }
    }
}
