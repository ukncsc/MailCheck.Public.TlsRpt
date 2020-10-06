GRANT SELECT, INSERT, UPDATE, DELETE ON `tls_rpt_entity` TO '{env}-tlsrpt-ent' IDENTIFIED BY '{password}';
GRANT SELECT ON `tls_rpt_entity` TO '{env}-tlsrpt-api' IDENTIFIED BY '{password}';