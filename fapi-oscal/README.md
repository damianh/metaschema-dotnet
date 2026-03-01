# FAPI 2.0 OSCAL Artifacts

This directory contains OSCAL (Open Security Controls Assessment Language) artifacts for the OpenID Foundation FAPI 2.0 specification family.

## Approach: Profile-Based Mapping

Rather than creating a standalone FAPI 2.0 catalog, we use OSCAL **profiles** to map FAPI 2.0 requirements to established security frameworks (NIST 800-53, ISO 27001). This approach provides:

- **GRC Tool Compatibility**: Most GRC tools natively support NIST and ISO frameworks
- **Auditor Familiarity**: Auditors are trained on established control frameworks
- **Traceability**: Clear mapping from FAPI requirements to authoritative controls
- **Compliance Reuse**: Organizations can leverage existing compliance work

## Structure

```
fapi-oscal/
├── profiles/
│   ├── fapi-2.0-nist-profile.json                # FAPI 2.0 Security Profile → NIST SP 800-53 Rev 5
│   ├── fapi-2.0-message-signing-nist-profile.json # FAPI 2.0 Message Signing → NIST (extends security profile)
│   ├── fapi-2.0-iso-profile.json                 # FAPI 2.0 → ISO/IEC 27002:2022
│   └── fapi-2.0-unified-profile.json             # Imports both NIST and ISO (planned)
└── README.md
```

## NIST 800-53 Profile

The `fapi-2.0-nist-profile.json` maps FAPI 2.0 Security Profile requirements to NIST SP 800-53 Rev 5 controls.

### Controls Selected

| Family | Controls | Description |
|--------|----------|-------------|
| AC | ac-3, ac-12 | Access Control, Session Termination |
| AU | au-9 | Audit Information Protection |
| IA | ia-5, ia-5.2, ia-8, ia-9 | Authenticator Management, PKI, Service Identification |
| SC | sc-8, sc-8.1, sc-12, sc-12.1, sc-13, sc-17, sc-20, sc-21, sc-22, sc-23 | System Communications Protection |
| SI | si-10 | Input Validation |

**Total: 18 controls**

### Profile Features

1. **Control Selection**: Imports 18 relevant NIST controls
2. **Parameter Settings**: Sets 5 FAPI-specific parameter values
3. **FAPI Guidance**: Adds FAPI-specific guidance to 13 controls
4. **Assessment Procedures**: Includes TEST and EXAMINE methods for each control
5. **FAPI-Specific Requirements**: 10 requirements with no direct NIST mapping

### FAPI-Specific Additions

Requirements that don't map directly to NIST controls are added as profile-specific parts:

| ID | Title | Attached To |
|----|-------|-------------|
| FAPI-DPOP-01 | DPoP Nonce Support | sc-23 |
| FAPI-DPOP-02 | Authorization Code Binding to DPoP Key | sc-23 |
| FAPI-JWT-01 | JWT Timestamp Tolerance | ia-5 |
| FAPI-MTLS-01 | MTLS Endpoint Aliases Support | ia-5 |
| FAPI-FLOW-01 | Response Type Code Only | ac-3 |
| FAPI-TOKEN-01 | Refresh Token Rotation Restrictions | ac-3 |
| FAPI-HTTP-01 | HTTP Redirect Status Codes | si-10 |
| FAPI-OIDC-01 | OIDC Nonce Length | si-10 |
| FAPI-PAR-01 | PAR Request URI Lifetime & One-Time Use | si-10 |
| FAPI-CSRF-01 | Authorization Flow Initiation Protection | si-10 |

### Assessment Methods

Each FAPI addition includes structured assessment procedures following NIST 800-53A patterns:

- **TEST**: Technical testing steps with specific test cases
- **EXAMINE**: Documentation and configuration review items

## Usage

### Loading the Profile

```csharp
using System.Text.Json;

var json = File.ReadAllText("fapi-oscal/profiles/fapi-2.0-nist-profile.json");
var doc = JsonDocument.Parse(json);
var profile = doc.RootElement.GetProperty("profile");

// Access metadata
var title = profile.GetProperty("metadata").GetProperty("title").GetString();

// Access controls to import
var imports = profile.GetProperty("imports")[0];
var controlIds = imports.GetProperty("include-controls")[0].GetProperty("with-ids");

// Access modifications (parameters and alterations)
var modify = profile.GetProperty("modify");
var setParams = modify.GetProperty("set-parameters");
var alters = modify.GetProperty("alters");
```

### Resolving the Profile

To get the resolved catalog (with all modifications applied), use an OSCAL profile resolver. The profile imports the NIST SP 800-53 Rev 5 catalog via the `back-matter` resource reference.

## ISO/IEC 27002:2022 Profile

The `fapi-2.0-iso-profile.json` maps FAPI 2.0 Security Profile requirements to ISO/IEC 27002:2022 controls.

> **Note**: An ISO 27002:2022 OSCAL catalog must be obtained or created separately due to ISO copyright restrictions. The profile references the catalog via back-matter but does not include the catalog content.

### Controls Selected

| Control | Title | FAPI Mapping |
|---------|-------|--------------|
| 5.15 | Access control | Sender-constrained tokens |
| 5.17 | Authentication information | Client authentication (MTLS/private_key_jwt) |
| 5.18 | Access rights | Token binding, PKCE, authorization codes |
| 8.2 | Privileged access rights | Confidential clients, PAR |
| 8.3 | Information access restriction | Resource server token validation |
| 8.5 | Secure authentication | JWT validation, iss parameter |
| 8.20 | Networks security | TLS requirements |
| 8.21 | Security of network services | Cipher suites, CORS |
| 8.24 | Use of cryptography | JWT algorithms, key sizes |
| 8.25 | Secure development lifecycle | Input validation, PAR, redirect URI |
| 8.26 | Application security requirements | Token handling, CSRF |

**Total: 11 controls**

### Profile Features

1. **Control Selection**: Imports 11 relevant ISO 27002:2022 controls
2. **FAPI Guidance**: Adds FAPI-specific guidance to each control
3. **Assessment Procedures**: Includes TEST methods for each control

## Message Signing Profile

The `fapi-2.0-message-signing-nist-profile.json` extends the Security Profile with Message Signing requirements for non-repudiation.

### Additional Controls

| Control | Description |
|---------|-------------|
| au-10 | Non-Repudiation |

### FAPI-Specific Additions

| ID | Title | Description |
|----|-------|-------------|
| FAPI-JAR-01 | Signed Authorization Request Validation | JAR (RFC 9101) support |
| FAPI-JARM-01 | Signed Authorization Response (JARM) | JWT Secured Authorization Response Mode |
| FAPI-INTRO-01 | Signed Introspection Response | RFC 9701 JWT introspection |

### Non-Repudiation Coverage

The Message Signing profile provides non-repudiation for:
- **NR1**: Pushed authorization requests (via JAR)
- **NR2**: Authorization requests - front-channel (via PAR + JAR)
- **NR3**: Authorization responses - front-channel (via JARM)
- **NR4**: Introspection responses (via RFC 9701)
- **NR5**: ID tokens (already signed per Security Profile)

## Source Specifications

- [FAPI 2.0 Security Profile](https://openid.net/specs/fapi-2_0-security-profile.html)
- [FAPI 2.0 Attacker Model](https://openid.net/specs/fapi-2_0-attacker-model.html)
- [FAPI 2.0 Message Signing](https://openid.net/specs/fapi-2_0-message-signing.html)
- [NIST SP 800-53 Rev 5](https://csrc.nist.gov/publications/detail/sp/800-53/rev-5/final)
- [ISO/IEC 27002:2022](https://www.iso.org/standard/75652.html)

## Requirement Levels

FAPI additions use requirement levels from RFC 2119 / ISO Directive Part 2:

| Level | Meaning |
|-------|---------|
| `shall` | Mandatory requirement |
| `shall not` | Mandatory prohibition |
| `should` | Recommended |
| `should not` | Not recommended |
| `may` | Optional |

## Version

- Profile Version: 1.0.0-draft
- OSCAL Version: 1.1.1
- Based on FAPI 2.0 specifications as of January 2026

## License

This OSCAL profile is derived from OpenID Foundation FAPI 2.0 specifications.
See the OpenID Foundation [IPR Policy](https://openid.net/intellectual-property/) for terms.
