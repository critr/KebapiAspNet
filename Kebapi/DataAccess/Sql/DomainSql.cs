using Kebapi.Domain;
using System.Collections.Generic;

namespace Kebapi.DataAccess.Sql
{

    /// <summary>
    /// Dal SQL for domain operation. 
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
    internal static class DomainSql
    {
        // Users.
        private static readonly string _commonUserSelectFields = @"
            u.[id], 
            u.[username],
            u.[name],
            u.[surname], 
            u.[email], 
            u.[password_hash], 
            lr.[role],
            u.[account_status_id]";
        internal static Dictionary<string, string> Users = new() 
        {
            { 
                "GetUser",
                @$"
                    SELECT {_commonUserSelectFields} 
                    FROM [{DbTable.Users}] u 
                    INNER JOIN [{DbTable.LookupRoles}] lr 
                    ON u.[role_id] = lr.[id] 
                    WHERE u.[id] = @id
                ;"
            },
            {
                "GetUserByUsername",
                @$"
                    SELECT {_commonUserSelectFields} 
                    FROM [{DbTable.Users}] u 
                    INNER JOIN [{DbTable.LookupRoles}] lr 
                    ON u.[role_id] = lr.[id] 
                    WHERE u.[username] = @username
                ;"
            },
            {
                "GetUserByEmail",
                @$"
                    SELECT {_commonUserSelectFields} 
                    FROM [{DbTable.Users}] u 
                    INNER JOIN [{DbTable.LookupRoles}] lr 
                    ON u.[role_id] = lr.[id] 
                    WHERE u.[email] = @email
                ;"
            },
            {
                "GetUserAccountStatus",
                @$"
                    SELECT uas.[id], uas.[status] 
                    FROM [{DbTable.LookupUserAccountStatus}] uas 
                    INNER JOIN [{DbTable.Users}] u 
                    ON uas.[id] = u.[account_status_id] 
                    WHERE u.[id] = @id
                ;"
            },
            {
                "GetUserCount",
                @$"SELECT Count([id]) FROM [{DbTable.Users}];"
            },
            {
                "GetUserFavourites",
                @$"
                    SELECT v.[id], v.[name], v.[geo_lat], v.[geo_lng], 
                        v.[address], v.[rating], m.[media_path] 
                    FROM [{DbTable.Venues}] v 
                    INNER JOIN [{DbTable.UserFavouriteVenues}] ufv 
                    ON v.[id] = ufv.[venue_id]
                    INNER JOIN [{DbTable.Media}] m 
                    ON m.[id] = v.[main_media_id]
                    WHERE ufv.[user_id] = @id 
                    ORDER BY v.[name] 
                    OFFSET @offset ROWS 
                    FETCH NEXT @limit ROWS ONLY
                ;"
            },
            {
                "GetUsers",
                @$"
                    SELECT {_commonUserSelectFields} 
                    FROM [{DbTable.Users}] u 
                    INNER JOIN [{DbTable.LookupRoles}] lr 
                    ON u.[role_id] = lr.[id] 
                    ORDER BY u.[id] 
                    OFFSET @offset ROWS 
                    FETCH NEXT @limit ROWS ONLY
                ;"
            },
            {
                "ActivateUser",
                @$"
                    UPDATE [{DbTable.Users}] 
                    SET [account_status_id] = {(int)User.AccountStatus.Active} 
                    WHERE [id] = @id
                ;"
            },
            {
                "DeactivateUser",
                @$"
                    UPDATE [{DbTable.Users}] 
                    SET [account_status_id] = {(int)User.AccountStatus.Inactive} 
                    WHERE [id] = @id
                ;"
            },
            {
                "AddUser",
                @$"
                    INSERT INTO [{DbTable.Users}] (
                        [username], [name], [surname], [email], [password_hash],
                        [role_id], [account_status_id]) 
	                OUTPUT INSERTED.[id]
	                VALUES (
                        @username, @name, @surname, @email, @passwordHash, 
                        @roleId, @accountStatusId)
                ;"
            },
            // Unused alternarive. Opted for simpler more intuitive AddUser instead.
            // Although this is nice in the sense it does everything in one go
            // (checks existence and inserts) and it doesn't depend on constraints.
            // It also requires running a similar statement later to work out why
            // something didn't insert.
            {
                "AddUserAlternative",
                @$"
                    WITH incoming(
                        [username], [name], [surname], [email], [password_hash],
                        [role_id], [account_status_id]) 
                    AS (
                        SELECT @username [username], @name [name], @surname [surname],
                            @email [email], @passwordHash [password_hash], 
                            @roleId [role_id], @accountStatusId [account_status_id]),
                    incomingWithExistsChecks(
                        [username], [name], [surname], [email], [password_hash], 
                        [role_id], [account_status_id], usernameExists, emailExists) 
                    AS (
                        SELECT i.*, un.[username], ue.[email] 
                        FROM incoming i 
                        LEFT OUTER JOIN [{DbTable.Users}] un 
                        ON un.[username] = i.[username] 
						LEFT OUTER JOIN [{DbTable.Users}] ue 
                        ON ue.[email] = i.[email])
                    INSERT INTO [{DbTable.Users}] (
                        [username], [name], [surname], [email], [password_hash],
                        [role_id], [account_status_id]) 
	                OUTPUT INSERTED.[id]
				    SELECT [username], [name], [surname], [email], [password_hash],
                        [role_id], [account_status_id] 
                    FROM incomingWithExistsChecks ie
                    WHERE ie.usernameExists IS Null
                    AND ie.emailExists IS Null
                ;"
            },
            {
                "AddUserFavourite",
                @$"
                    INSERT INTO [{DbTable.UserFavouriteVenues}] (
                        [user_id], [venue_id]) 
                    OUTPUT INSERTED.id 
                    VALUES (@id, @venueId)
                ;"
            },
            {
                "RemoveUserFavourite",
                @$"
                    DELETE 
                    FROM [{DbTable.UserFavouriteVenues}] 
                    OUTPUT DELETED.id 
                    WHERE [user_id] = @id 
                    AND [venue_id] = @venueId
                ;"
            },
        };


        // Venues.
        private static readonly string _commonVenueSelectFields = @"
            v.[id],
            v.[name],
            v.[geo_lat],
            v.[geo_lng],
            v.[address],
            v.[rating],
            m.[media_path] AS main_media_path
        ";
        internal static Dictionary<string, string> Venues = new() 
        {
            {
                "GetVenue",
                @$"
                    SELECT
                        {_commonVenueSelectFields}
                    FROM {DbTable.Venues} AS v
                    INNER JOIN {DbTable.Media} AS m
                    ON v.[main_media_id] = m.[id]
                    WHERE v.[id] = @id
                ;"
            },
            {
                "GetVenues",
                @$"
                    SELECT
                            {_commonVenueSelectFields}
                    FROM {DbTable.Venues} AS v
                    INNER JOIN {DbTable.Media} AS m
                    ON v.[main_media_id] = m.[id]
                    ORDER BY v.[id] 
                    OFFSET @offset ROWS 
                    FETCH NEXT @limit ROWS ONLY
                ;"
            },
            {
                "GetVenueCount",
                $"SELECT Count([id]) FROM [{DbTable.Venues}];"
            },

        };

    }
}
