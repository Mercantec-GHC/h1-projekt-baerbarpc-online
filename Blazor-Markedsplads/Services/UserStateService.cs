namespace BlazorMarkedsplads.Services 
{
    public class UserStateService
    {
        // Properties til at holde den aktuelle brugers tilstand.
        // 'private set' sikrer, at de kun kan ændres indefra denne service.
        public bool IsLoggedIn { get; private set; }
        public int? UserId { get; private set; }

        // Et event, som andre komponenter kan abonnere på.
        // Hver gang login-status ændres, udløses dette event.
        public event Action? OnChange;

        // Metode til at kalde, når en bruger logger ind
        public void Login(int userId)
        {
            IsLoggedIn = true;
            UserId = userId;

            // TRIN 1: Tilføj denne linje
            Console.WriteLine("[DEBUG] UserStateService.Login() blev kaldt. Udløser OnChange event.");

            NotifyStateChanged();
        } 

        // Metode til at kalde, når en bruger logger ud
        public void Logout()
        {
            IsLoggedIn = false;
            UserId = null;
            NotifyStateChanged();
        }


        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}