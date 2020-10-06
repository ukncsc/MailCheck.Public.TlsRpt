GRANT SELECT, INSERT, UPDATE ON `tls_rpt_entity_history` TO '{env}-tlsrpt-ent' IDENTIFIED BY '{password}';
GRANT SELECT, INSERT, UPDATE, DELETE ON `tls_rpt_entity` TO '{env}-tlsrpt-ent' IDENTIFIED BY '{password}';

GRANT SELECT, INSERT, UPDATE, DELETE ON `tls_rpt_scheduled_records` TO '{env}-tlsrpt-sch' IDENTIFIED BY '{password}';

GRANT SELECT ON `tls_rpt_entity` TO '{env}-tlsrpt-api' IDENTIFIED BY '{password}';
GRANT SELECT ON `tls_rpt_entity_history` TO '{env}-tlsrpt-api' IDENTIFIED BY '{password}';

GRANT SELECT ON `tls_rpt_entity` TO '{env}_reports' IDENTIFIED BY '{password}';
GRANT SELECT INTO S3 ON *.* TO '{env}_reports' IDENTIFIED BY '{password}';