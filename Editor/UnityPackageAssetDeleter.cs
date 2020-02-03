﻿using SharpCompress.Readers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace KoganeUnityEditorLib
{
	public static class UnityPackageAssetDeleter
	{
		[MenuItem( "Assets/Delete .unitypackage Assets" )]
		private static void Run()
		{
			var path = EditorUtility.OpenFilePanel( string.Empty, string.Empty, "unitypackage" );

			if ( !path.EndsWith( ".unitypackage" ) ) return;
			if ( string.IsNullOrWhiteSpace( path ) ) return;

			var unityPackageFilename = Path.GetFileName( path );
			var unityPackageGuidList = GetGuidListFromUnityPackage( path );

			var unityPackageAssetPathList = unityPackageGuidList
					.Select( AssetDatabase.GUIDToAssetPath )
					.Where( c => !string.IsNullOrWhiteSpace( c ) )
					.ToArray()
				;

			var count = unityPackageAssetPathList.Length;
			for ( var i = 0; i < count; i++ )
			{
				var progress  = ( float ) i / count;
				var assetPath = unityPackageAssetPathList[ i ];
				AssetDatabase.DeleteAsset( assetPath );
				EditorUtility.DisplayProgressBar
				(
					title: $"Delete {unityPackageFilename} Assets",
					info: $"{i + 1}/{count}",
					progress: progress
				);
			}

			EditorUtility.ClearProgressBar();

			DoDelete( "Assets" );

			AssetDatabase.Refresh();
		}

		private static void DoDelete( string path )
		{
			foreach ( var dir in Directory.GetDirectories( path ) )
			{
				DoDelete( dir );

				var files = Directory.GetFiles( dir );

				if ( files.Length != 0 ) continue;

				var dirs = Directory.GetDirectories( dir );

				if ( dirs.Length != 0 ) continue;

				AssetDatabase.DeleteAsset( dir );
			}
		}

		private static IEnumerable<string> GetGuidListFromUnityPackage( string path )
		{
			var regex = new Regex( @"[^a-zA-z0-9]" );

			using ( var stream = File.OpenRead( path ) )
			{
				var reader = ReaderFactory.Open( stream );

				while ( reader.MoveToNextEntry() )
				{
					var entry = reader.Entry;

					if ( entry.IsDirectory )
					{
						yield return regex.Replace( entry.Key, string.Empty );
					}
				}
			}
		}
	}
}