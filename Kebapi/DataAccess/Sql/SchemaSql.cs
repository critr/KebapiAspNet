using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kebapi.DataAccess.Sql
{
    /// <summary>
    /// Dal SQL for setup of the database schema.
    /// </summary>
    // These could go into a resource file (resx) to allow changes to the SQL
    // without recompilation. However resource files really are more geared to
    // i18n, their UI isn't great for SQL, it limits string interpolation and
    // other language editing features like syntax highlighting, it's "yet another
    // place" where SQL might be stored, and it adds another layer that the
    // framework must process and deal with (no matter how optimised).
    //
    // If avoiding recompilation of the entire app is a must, these could go in
    // a separate assembly. Arguably however, changes to core SQL aren't likely
    // to be off the cuff affairs requiring little oversight.
    //
    // Another alternative is to implement as stored procedures on the database
    // server, but that brings other advantages/disadvantages.
    internal static class SchemaSql
    {
        internal static Dictionary<string, string> Create = new()
        {
            {
                $"Table.{DbTable.LookupRoles}",
                $"CREATE TABLE [{DbTable.LookupRoles}]([id] [TINYINT] NOT NULL, [role] [VARCHAR](20) NOT NULL, CONSTRAINT [pk_lookup_roles] PRIMARY KEY CLUSTERED ([id] ASC), CONSTRAINT [uq_lookup_roles_role] UNIQUE ([role] ASC));"
            },
            {
                $"Table.{DbTable.LookupUserAccountStatus}",
                $"CREATE TABLE [{DbTable.LookupUserAccountStatus}]([id] [TINYINT] NOT NULL, [status] [VARCHAR](20) NULL, CONSTRAINT [pk_lookup_user_account_status] PRIMARY KEY CLUSTERED ([id] ASC), CONSTRAINT [uq_lookup_user_account_status_status] UNIQUE ([status] ASC));"
            },
            {
                $"Table.{DbTable.Users}",
                $"CREATE TABLE [{DbTable.Users}]([id] [INT] IDENTITY(1,1) NOT NULL, [username] [VARCHAR](40) NOT NULL, [name] [VARCHAR](50) NOT NULL, [surname] [VARCHAR](50) NOT NULL, [email] [VARCHAR](320) NOT NULL, [password_hash] [CHAR](120) NOT NULL, [role_id] [TINYINT] NOT NULL, [account_status_id] [TINYINT] NOT NULL, CONSTRAINT [pk_users] PRIMARY KEY CLUSTERED ([id] ASC), CONSTRAINT [fk_users_lookup_roles] FOREIGN KEY([role_id]) REFERENCES [dbo].[lookup_roles] ([id]), CONSTRAINT [fk_users_lookup_user_account_status] FOREIGN KEY([account_status_id]) REFERENCES [dbo].[lookup_user_account_status] ([id]), CONSTRAINT [uq_user_username] UNIQUE ([username] ASC), CONSTRAINT [uq_user_email] UNIQUE ([email] ASC));"
            },
            {
                $"Table.{DbTable.Media}",
                $"CREATE TABLE [{DbTable.Media}]([id] [INT] IDENTITY(1,1) NOT NULL, [user_id] [INT] NOT NULL, [media_path] [VARCHAR](1000) NOT NULL, CONSTRAINT [pk_media] PRIMARY KEY CLUSTERED ([id] ASC), CONSTRAINT [fk_media_users] FOREIGN KEY([user_id]) REFERENCES [dbo].[users] ([id]));"
            },
            {
                $"Table.{DbTable.Venues}",
                $"CREATE TABLE [{DbTable.Venues}]([id] [INT] IDENTITY(1,1) NOT NULL, [name] [VARCHAR](255) NOT NULL, [geo_lat] [DECIMAL](8, 6) NULL, [geo_lng] [DECIMAL](9, 6) NULL, [address] [VARCHAR](255) NULL, [rating] [TINYINT] NULL,	[main_media_id] [INT] NOT NULL, CONSTRAINT [pk_venues] PRIMARY KEY CLUSTERED ([id] ASC), CONSTRAINT [fk_venues_media_id] FOREIGN KEY([main_media_id]) REFERENCES [dbo].[media] ([id]));"
            },
            {
                $"Table.{DbTable.UserFavouriteVenues}",
                $"CREATE TABLE [{DbTable.UserFavouriteVenues}]([id] [INT] IDENTITY(1,1) NOT NULL, [user_id] [INT] NOT NULL, [venue_id] [INT] NOT NULL, CONSTRAINT [pk_user_favourite_venues] PRIMARY KEY CLUSTERED ([id] ASC),CONSTRAINT [fk_user_favourite_venues_users_id] FOREIGN KEY([user_id]) REFERENCES [dbo].[users] ([id]),CONSTRAINT [fk_user_favourite_venues_venues_id] FOREIGN KEY([venue_id]) REFERENCES [dbo].[venues] ([id]), CONSTRAINT [uq_user_venue] UNIQUE NONCLUSTERED ([user_id], [venue_id]));"
            },
        };
    }
}
