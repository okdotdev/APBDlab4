using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly ClientRepository _clientRepository = new();
        private readonly UserCreditService _userCreditService = new();
        private const int MinimumCreditLimit = 500;

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!ValidateUserInput(firstName, lastName, email, dateOfBirth))
                return false;

            var client = _clientRepository.GetById(clientId);
            var user = CreateNewUser(client, dateOfBirth, email, firstName, lastName);

            SetUserCreditLimit(user, client);

            if (!CheckUserCreditLimit(user))
                return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private static bool ValidateUserInput(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            return CheckIfUserProvidedHisFirstNameAndLastName(firstName, lastName) &&
                   CheckIfEmailIsValid(email) &&
                   CheckIfUserIsOlderThan21(dateOfBirth);
        }

        private static bool CheckIfEmailIsValid(string email)
        {
            return email.Contains('@') && email.Contains('.');
        }

        private static bool CheckIfUserProvidedHisFirstNameAndLastName(string firstName, string lastName)
        {
            return !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName);
        }

        private static bool CheckIfUserIsOlderThan21(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            var age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            return age >= 21;
        }

        private static User CreateNewUser(Client client, DateTime dateOfBirth, string email, string firstName,
            string lastName)
        {
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            return user;
        }

        private void SetUserCreditLimit(User user, Client client)
        {
            switch (client.Type)
            {
                case ClientType.VeryImportantClient:
                    user.HasCreditLimit = false;
                    break;
                case ClientType.ImportantClient:
                {
                    var creditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    creditLimit *= 2;
                    user.CreditLimit = creditLimit;
                    break;
                }
                case ClientType.NormalClient:
                {
                    user.HasCreditLimit = true;
                    var creditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(client.Type.ToString());
            }
        }

        private static bool CheckUserCreditLimit(User user)
        {
            return !user.HasCreditLimit || user.CreditLimit > MinimumCreditLimit;
        }
    }
}