namespace DataStore.Infrastructure.PureFunctions
{
    using PWDTK_DOTNET451;

    /// <summary>
    ///     This is a HMACSHA512 implementation of PBKDF2 With a 512-bit(64 bytes) default salt and variable iterations
    /// </summary>
    public class SecureHmacHash
    {
        public const int CurrentIterations = 10000;

        public string HexHash { get; set; }

        public string HexSalt { get; set; }

        public int IterationsUsed { get; set; }

        public static SecureHmacHash Create(string textString)
        {
            return Create(textString, CurrentIterations, PWDTK.GetRandomSalt());
        }

        public static SecureHmacHash CreateFrom(string password, int iterations, string saltHex)
        {
            return Create(password, iterations, PWDTK.HashHexStringToBytes(saltHex));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((SecureHmacHash)obj);
        }

        public bool EqualsPassword(string textString)
        {
            return PWDTK.ComparePasswordToHash(
                PWDTK.HashHexStringToBytes(this.HexSalt), 
                textString, 
                PWDTK.HashHexStringToBytes(this.HexHash), 
                CurrentIterations);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.HexSalt != null ? this.HexSalt.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ this.IterationsUsed;
                hashCode = (hashCode * 397) ^ (this.HexHash != null ? this.HexHash.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{this.HexSalt}:{this.HexHash}:{this.IterationsUsed}:";
        }

        protected bool Equals(SecureHmacHash other)
        {
            return string.Equals(this.HexSalt, other.HexSalt) && this.IterationsUsed == other.IterationsUsed
                   && string.Equals(this.HexHash, other.HexHash);
        }

        private static SecureHmacHash Create(string password, int iterations, byte[] saltBytes)
        {
            var passwordHashHex = PWDTK.PasswordToHashHexString(saltBytes, password, iterations);
            var saltHex = PWDTK.HashBytesToHexString(saltBytes);

            return new SecureHmacHash { HexHash = passwordHashHex, HexSalt = saltHex, IterationsUsed = iterations };
        }
    }
}