namespace Biscuit.Errors
{
    public class FailedCheck
    {
        public class FailedBlock : FailedCheck
        {
            public long block_id;
            public long caveat_id;
            public string rule;

            public FailedBlock(long block_id, long caveat_id, string rule)
            {
                this.block_id = block_id;
                this.caveat_id = caveat_id;
                this.rule = rule;
            }

            public override bool Equals(object obj)
            {
                if (this == obj) return true;
                if (obj == null || !(obj is FailedBlock)) return false;
                FailedBlock b = (FailedBlock)obj;
                return block_id == b.block_id && caveat_id == b.caveat_id && rule.Equals(b.rule);
            }

            public override int GetHashCode()
            {
                return Objects.hash(block_id, caveat_id, rule);
            }

            public override string ToString()
            {
                return "FailedCaveat.FailedBlock { block_id: " + block_id + ", caveat_id: " + caveat_id +
                   ", rule: " + rule + " }";
            }
        }

        public class FailedVerifier : FailedCheck
        {
            public long caveat_id;
            public string rule;

            public FailedVerifier(long caveat_id, string rule)
            {
                this.caveat_id = caveat_id;
                this.rule = rule;
            }
            public override bool Equals(object obj)
            {
                if (this == obj) return true;
                if (obj == null || !(obj is FailedVerifier)) return false;
                FailedVerifier b = (FailedVerifier)obj;
                return caveat_id == b.caveat_id && rule.Equals(b.rule);
            }

            public override int GetHashCode()
            {
                return Objects.hash(caveat_id, rule);
            }

            public override string ToString()
            {
                return "FailedCaveat.FailedVerifier { caveat_id: " + caveat_id +
                        ", rule: " + rule + " }";
            }
        }
    }
}
