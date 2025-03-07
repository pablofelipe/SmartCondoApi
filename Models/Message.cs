namespace SmartCondoApi.Models
{
    public partial class Message
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }


        //Relacionamento com Users(quem enviou).
        public int SenderId { get; set; }
        public User SenderUser { get; set; }

        public int CondominiumId { get; set; }
        public Condominium Condominium { get; set; }

        public int? TowerId { get; set; }
        public Tower Tower { get; set; }

        public int? FloorId { get; set; }

        //Mensagem para outro usuário do sistema
        public int? RecipientId { get; set; }
        public User RecipientUser { get; set; }
    }
}
