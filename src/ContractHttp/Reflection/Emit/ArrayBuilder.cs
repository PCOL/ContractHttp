namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Reflection.Emit;

    /// <summary>
    /// Generates the IL for handling an array.
    /// </summary>
    public class ArrayBuilder
    {
        private ILGenerator ilGenerator;

        private LocalBuilder localVar;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayBuilder"/> class.
        /// </summary>
        /// <param name="ilGenerator">The IL generator.</param>
        /// <param name="arrayType">The type of array.</param>
        /// <param name="length">The length of the array.</param>
        /// <param name="localBuilder">Optional local builder.</param>
        public ArrayBuilder(ILGenerator ilGenerator, Type arrayType, int length, LocalBuilder localBuilder = null)
        {
            this.ilGenerator = ilGenerator;
            this.localVar = localBuilder ?? this.ilGenerator.DeclareLocal(arrayType.MakeArrayType());

            // Create the argument types array.
            this.ilGenerator.Emit(OpCodes.Ldc_I4, length);
            this.ilGenerator.Emit(OpCodes.Newarr, arrayType);
            this.ilGenerator.Emit(OpCodes.Stloc, this.localVar);
        }

        /// <summary>
        /// Emits the IL to start a set operation for the given index.
        /// </summary>
        /// <param name="index">The array index to set.</param>
        public void EmitSetStart(int index)
        {
            this.ilGenerator.Emit(OpCodes.Ldloc, this.localVar);
            this.ilGenerator.Emit(OpCodes.Ldc_I4, index);
        }

        /// <summary>
        /// Emits the IL to store the element.
        /// </summary>
        public void EmitSetEnd()
        {
            this.ilGenerator.Emit(OpCodes.Stelem_Ref);
        }

        /// <summary>
        /// Emits the IL to set an element of an array.
        /// </summary>
        /// <param name="index">The index of the element to set.</param>
        /// <param name="action">The action to call to emit the set code.</param>
        public void EmitSet(int index, Action<ILGenerator> action)
        {
            this.EmitSetStart(index);

            if (action != null)
            {
                action(this.ilGenerator);
            }

            this.EmitSetEnd();
        }

        /// <summary>
        /// Emits the IL to set an element of an array.
        /// </summary>
        /// <param name="index">The index of the element to set.</param>
        /// <param name="action">The action to call to emit the set code.</param>
        public void EmitSet(int index, Action<ILGenerator, int> action)
        {
            this.EmitSetStart(index);

            if (action != null)
            {
                action(this.ilGenerator, index);
            }

            this.EmitSetEnd();
        }

        /// <summary>
        /// Emits the IL to load the given array element onto the evaluation stack.
        /// </summary>
        /// <param name="index">The index of the element to load.</param>
        public void EmitGet(int index)
        {
            this.ilGenerator.Emit(OpCodes.Ldloc, this.localVar);
            this.ilGenerator.Emit(OpCodes.Ldc_I4, index);
            this.ilGenerator.Emit(OpCodes.Ldelem_Ref);
        }

        /// <summary>
        /// Emits the IL to load the array onto the evaluation stack.
        /// </summary>
        public void EmitLoad()
        {
            this.ilGenerator.Emit(OpCodes.Ldloc, this.localVar);
        }
    }
}
