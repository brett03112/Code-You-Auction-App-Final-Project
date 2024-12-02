﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AuctionApp.Models;

[Index("IdentityUserId", Name = "IX_Listings_IdentityUserId")]
public partial class Listing
{
    [Key]
    public int ListingId { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public string Title { get; set; }

    public string ImagePath { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    public string Description { get; set; }

    [Column(TypeName = "money")]
    public decimal HighestBid { get; set; }

    [Required]
    public string WinningBidder { get; set; }

    [Required]
    public string IdentityUserId { get; set; }

    [InverseProperty("Listing")]
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    [ForeignKey("IdentityUserId")]
    [InverseProperty("Listings")]
    public virtual IdentityUser IdentityUser { get; set; }
}