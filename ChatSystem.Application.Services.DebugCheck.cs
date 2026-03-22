// run from a debug path to confirm the stored value
var user = await _userManager.FindByIdAsync("049f751c-4fcb-47d0-9457-fe4536a3380b");
Console.WriteLine($"ProfileImage: '{user?.ProfileImage ?? "NULL"}'");