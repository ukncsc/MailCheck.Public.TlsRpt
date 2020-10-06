GRANT SELECT, INSERT, UPDATE ON `tls_rpt_entity_history` TO '{env}-tlsrpt-ent' IDENTIFIED BY '{password}';
GRANT SELECT ON `tls_rpt_entity_history` TO '{env}-tlsrpt-api' IDENTIFIED BY '{password}';