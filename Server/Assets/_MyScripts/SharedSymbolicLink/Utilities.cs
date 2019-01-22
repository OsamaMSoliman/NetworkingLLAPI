using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public static class Utilities
{
	public const string EMAIL_PATTERN = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
	public const string USERNAME_AND_DISCRIMINATOR_PATTERN = @"^[a-zA-Z0-9]{4,20}#[0-9]{4}$";
	public const string USERNAME_PATTERN = @"^[a-zA-Z0-9]{4,20}$";
	public const string RANDOM_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
	private static Random r = new Random();

	public static bool IsEmail(string email) { return email != null && Regex.IsMatch(email, EMAIL_PATTERN); }
	public static bool IsUsername(string username) { return username != null && Regex.IsMatch(username, USERNAME_PATTERN); }
	public static bool IsUsernameAndDiscriminator(string usernameAndDiscriminator) { return usernameAndDiscriminator != null && Regex.IsMatch(usernameAndDiscriminator, USERNAME_AND_DISCRIMINATOR_PATTERN); }
	public static string GenerateRandom(int length) { return new string(Enumerable.Repeat(RANDOM_CHARS, length).Select(s => s[r.Next(s.Length)]).ToArray()); }

	public static string SHA256(string password)
	{
		string hex = "";
		var hashValue = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(password));
		foreach (byte x in hashValue)
			hex += string.Format("{0:x2}", x);
		return hex;
	}

}
