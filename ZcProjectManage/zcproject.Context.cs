﻿

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace ZcProjectManage
{

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;


public partial class zc_project_collectionEntities : DbContext
{
    public zc_project_collectionEntities()
        : base("name=zc_project_collectionEntities")
    {

    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        throw new UnintentionalCodeFirstException();
    }


    public virtual DbSet<Company> Company { get; set; }

    public virtual DbSet<user> user { get; set; }

    public virtual DbSet<projectype> projectype { get; set; }

    public virtual DbSet<model> model { get; set; }

    public virtual DbSet<project> project { get; set; }

}

}
