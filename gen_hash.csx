using BCrypt.Net;
var hash = BCrypt.HashPassword("123456");
System.Console.WriteLine(hash);
