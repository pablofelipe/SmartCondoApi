namespace SmartCondoApi.Models
{
    public partial class Message
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }


        //Relacionamento com Users(quem enviou).
        public long SenderId { get; set; }
        public UserProfile SenderUser { get; set; }

        public int CondominiumId { get; set; }
        public Condominium Condominium { get; set; }

        public int? TowerId { get; set; }
        public Tower Tower { get; set; }

        public int? FloorId { get; set; }

        //Mensagem para outro usuário do sistema
        public long? RecipientId { get; set; }
        public UserProfile RecipientUser { get; set; }
    }
}
