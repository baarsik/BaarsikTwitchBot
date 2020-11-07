using System;
using System.ComponentModel.DataAnnotations;

namespace BaarsikTwitchBot.Domain.Models.Abstract
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}