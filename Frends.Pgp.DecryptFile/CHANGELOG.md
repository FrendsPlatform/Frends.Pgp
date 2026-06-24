# Changelog

## [1.3.0] - 2026-06-23

### Fixed

- Decryption failing with non-ASCII passphrases
- Output directory not created automatically

### Added

- Input.PrivateKeyString — option to provide the private key directly as a string instead of a file path
- Input.PassphraseEncoding — option to choose between Utf8 (default) and Legacy passphrase encoding

## [1.2.0] - 2026-05-11

### Fixed

- Support decryption of signed messages

## [1.1.0] - 2026-02-23

### Fixed

- Ensure FrendsTaskMetadata.json is included in NuGet package

## [1.0.0] - 2026-01-09

### Added

- Initial implementation
