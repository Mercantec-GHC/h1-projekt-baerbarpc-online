namespace BlazorMarkedsplads.Services
{
    /// <summary>
    /// En central service til at håndtere og dele brugerens login-status på tværs af hele applikationen.
    /// Denne service registreres som 'Scoped' i Program.cs, hvilket betyder, at der oprettes én enkelt instans
    /// for hver bruger-session (dvs. for hver åben fane i browseren).
    /// Alle komponenter inden for den samme session får adgang til den PRÆCIS SAMME instans af denne service,
    /// hvilket gør det muligt at dele data (state) mellem dem.
    /// </summary>
    public class UserStateService
    {
        // ------------------- PROPERTIES (State) -------------------
        // Properties holder den delte tilstand.

        /// <summary>
        /// Holder styr på, om en bruger er logget ind. Kan læses af alle, men kan kun ændres indefra denne klasse.
        /// </summary>
        public bool IsLoggedIn { get; private set; }

        /// <summary>
        /// Holder den indloggede brugers unikke ID. Er 'null', hvis ingen er logget ind.
        /// 'private set' sikrer, at værdien kun kan ændres via Login() og Logout() metoderne.
        /// </summary>
        public int? UserId { get; private set; }

        // ------------------- EVENT -------------------
        // Et event fungerer som et meddelelsessystem eller en "broadcast-signal".

        /// <summary>
        /// Et event, som andre dele af applikationen (f.eks. MainLayout.razor) kan abonnere på.
        /// Når dette event udløses, bliver alle abonnenter notificeret om, at login-statussen har ændret sig,
        /// så de kan opdatere deres UI. Dette er kernen i Observer-designmønsteret.
        /// </summary>
        public event Action? OnChange;

        // ------------------- PUBLIC METHODS -------------------
        // Offentlige metoder, som andre dele af koden kan kalde.

        /// <summary>
        /// Kaldes fra Login.razor efter en succesfuld login-verificering.
        /// Opdaterer den delte state og notificerer alle abonnenter.
        /// </summary>
        /// <param name="userId">ID'et på den bruger, der netop er logget ind.</param>
        public void Login(int userId)
        {
            // Opdater den interne state.
            IsLoggedIn = true;
            UserId = userId;

            // Udløs OnChange-eventet for at informere resten af appen om ændringen.
            NotifyStateChanged();
        }

        /// <summary>
        /// Kaldes fra UserProfile.razor, når brugeren klikker på "Log ud".
        /// Nulstiller den delte state og notificerer alle abonnenter.
        /// </summary>
        public void Logout()
        {
            // Nulstil den interne state.
            IsLoggedIn = false;
            UserId = null;

            // Udløs OnChange-eventet for at informere resten af appen om ændringen.
            NotifyStateChanged();
        }

        // ------------------- PRIVATE HELPER METHOD -------------------
        // En privat metode, der kun kan kaldes indefra denne klasse.

        /// <summary>
        /// En privat hjælpe-metode, der er ansvarlig for at udløse OnChange-eventet.
        /// </summary>
        private void NotifyStateChanged()
        {
            // 'OnChange?.Invoke()' er en sikker måde at udløse eventet på.
            // '?.' (null-conditional operator) sikrer, at .Invoke() kun kaldes, hvis der rent faktisk
            // er nogen abonnenter (listeners). Dette forhindrer en NullReferenceException.
            OnChange?.Invoke();
        }
    }
}